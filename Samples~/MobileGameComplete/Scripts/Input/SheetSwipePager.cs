using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EnhancedUI;
using EnhancedUI.Transition;

namespace EnhancedUI.Demo.Input
{
    /// <summary>
    /// Drag-to-follow + snap-to-page tab pager for <see cref="SheetContainer"/>.
    ///
    /// Replaces the legacy threshold-based SwipeDetector. Behaviour mirrors
    /// iOS UIPageViewController / Android ViewPager2:
    ///   • Pointer follows finger 1:1 (configurable ratio).
    ///   • Adjacent sheet is activated and pinned alongside the current one.
    ///   • Release snaps to current OR neighbor based on dual-trigger:
    ///       position threshold (% width) OR velocity threshold (px/s).
    ///   • Rubber-band resistance at boundaries (configurable).
    ///   • Vertical-axis lock defers gesture to nested ScrollRect (when wired).
    ///
    /// Requires a <see cref="Graphic"/> on the same GameObject so the
    /// EventSystem can raycast drag events. A transparent <see cref="Image"/>
    /// is added automatically if missing.
    ///
    /// Horizontal-only by design (portrait mobile sample target). For
    /// landscape vertical paging, derive or write a sibling component.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class SheetSwipePager : UIBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IInitializePotentialDragHandler
    {
        #region Inspector

        [Header("References")]
        [Tooltip("Sheet container that owns the registered tab sheets.")]
        [SerializeField] private SheetContainer sheetContainer;

        [Tooltip("Ordered list of sheet identifiers that form the tab strip. " +
                 "Order determines swipe direction (index 0 = leftmost).")]
        [SerializeField]
        private List<string> tabSheetIds = new List<string>
        {
            "HomeSheet",
            "BattleSheet",
            "InventorySheet",
            "ShopSheet"
        };

        [Header("Drag Behaviour")]
        [Tooltip("Finger-to-content follow ratio. 1.0 = 1:1 (default), " +
                 "below 1.0 makes content drag slower than the finger.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float followRatio = 1f;

        [Tooltip("Resistance applied when dragging past the first/last tab. " +
                 "0 = hard stop, 1 = no resistance. 0.4 mimics iOS rubber-band.")]
        [Range(0f, 1f)]
        [SerializeField] private float edgeResistance = 0.4f;

        [Tooltip("Pixel travel before the gesture commits to an axis. " +
                 "If the gesture is vertical-dominant the pager releases " +
                 "(use this together with a nested vertical ScrollRect).")]
        [SerializeField] private float axisLockThreshold = 10f;

        [Header("Snap Behaviour")]
        [Tooltip("Drag distance, as a fraction of viewport width, that " +
                 "commits to the neighbor tab. 0.25 = quarter-screen.")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float positionThreshold = 0.25f;

        [Tooltip("Release velocity (px/s) that flicks to the neighbor tab " +
                 "regardless of drag distance.")]
        [SerializeField] private float velocityThreshold = 800f;

        [Tooltip("Duration of the snap tween (seconds).")]
        [SerializeField] private float snapDuration = 0.3f;

        [Tooltip("Easing used by the snap tween.")]
        [SerializeField] private EaseType snapEase = EaseType.QuarticEaseOut;

        [Tooltip("If true, very fast flicks can skip multiple tabs. " +
                 "Default false (matches iOS/Android/Material guidance).")]
        [SerializeField] private bool allowFlickSkip = false;

        [Tooltip("Velocity multiplier used when allowFlickSkip is enabled. " +
                 "1.0 = each (velocityThreshold) px/s adds one tab.")]
        [SerializeField] private float flickSkipScale = 1f;

        [Header("Lifecycle")]
        [Tooltip("Ignore drag input while the framework is running its own " +
                 "transition (recommended).")]
        [SerializeField] private bool disableDuringFrameworkTransition = true;

        [Tooltip("Wrap from last tab to first (and vice-versa). " +
                 "Default false — tab navigation should stay predictable.")]
        [SerializeField] private bool wrapAround = false;

        #endregion

        #region State

        private RectTransform _viewport;
        private Graphic _raycastTarget;

        private int _currentIndex;
        private int _neighborIndex = -1;

        private bool _isDragging;
        private bool _axisDecided;
        private bool _gestureCommitted;       // true once axisLockThreshold was crossed horizontally

        private float _dragAccumX;            // finger delta x since drag start (raw)
        private float _dragAccumY;            // finger delta y since drag start (raw)
        private float _appliedOffsetX;        // pixels currently applied to current sheet

        private Vector2 _lastFramePosition;
        private float _lastFrameTime;
        private float _velocityX;

        private Coroutine _snapRoutine;

        #endregion

        #region Public API

        /// <summary>
        /// True while a finger is dragging the pager. Useful for blocking
        /// other input or pausing background animations.
        /// </summary>
        public bool IsDragging => _isDragging;

        /// <summary>
        /// True while the post-release snap tween is running.
        /// </summary>
        public bool IsSnapping => _snapRoutine != null;

        /// <summary>
        /// Identifier of the tab the pager considers active. Updated at the
        /// end of a snap.
        /// </summary>
        public string CurrentSheetId =>
            (_currentIndex >= 0 && _currentIndex < tabSheetIds.Count)
                ? tabSheetIds[_currentIndex]
                : null;

        /// <summary>
        /// Raised after a snap completes and SheetContainer state is in sync.
        /// </summary>
        public event System.Action<string> OnTabChanged;

        /// <summary>
        /// Programmatically navigate to a tab using the pager's snap visuals
        /// (same look as a finger flick). Use this from BottomTabBar buttons
        /// so click navigation matches swipe navigation.
        /// </summary>
        public void JumpToTab(string sheetId, bool animate = true)
        {
            if (sheetContainer == null) return;

            int targetIndex = tabSheetIds.IndexOf(sheetId);
            if (targetIndex < 0)
            {
                Debug.LogWarning($"[SheetSwipePager] JumpToTab: sheet id '{sheetId}' " +
                                 "is not in tabSheetIds.");
                return;
            }

            if (targetIndex == _currentIndex && !_isDragging) return;

            if (_isDragging || IsSnapping)
            {
                // Cancel any in-progress interaction cleanly.
                CancelSnap();
                ResetDragState();
            }

            if (!animate)
            {
                ApplyTabImmediate(targetIndex);
                return;
            }

            // Stage the target sheet adjacent to current so the snap tween
            // looks identical to a manual drag-release.
            int direction = ResolveJumpDirection(_currentIndex, targetIndex);
            StageNeighbor(_currentIndex + direction, /* applyOffset = */ true);

            // Bias the offset toward the target so the tween snaps to it.
            // (Tween picks whichever neighbor matches the offset sign.)
            float viewportWidth = GetViewportWidth();
            _appliedOffsetX = -direction * viewportWidth * 0.001f; // tiny nudge
            ApplyOffset(_appliedOffsetX);

            // Override neighbor to be the actual target (not just adjacent)
            // when allowFlickSkip is true or wrap-around is in play.
            _neighborIndex = WrapIndex(_currentIndex + direction);

            // Walk one step at a time so that intermediate sheets are not
            // skipped visually. For multi-step jumps we recurse after each.
            if (Mathf.Abs(targetIndex - _currentIndex) > 1 && !wrapAround)
            {
                StartSnap(direction, onComplete: () => JumpToTab(sheetId, true));
            }
            else
            {
                StartSnap(direction);
            }
        }

        #endregion

        #region Unity lifecycle

        protected override void Awake()
        {
            base.Awake();
            _viewport = (RectTransform)transform;
            EnsureRaycastTarget();
        }

        protected override void Start()
        {
            base.Start();
            // Sync index with whatever sheet the container is currently showing.
            if (sheetContainer != null && sheetContainer.ActiveSheet != null)
            {
                string id = sheetContainer.ActiveSheet.Identifier;
                int idx = tabSheetIds.IndexOf(id);
                if (idx >= 0) _currentIndex = idx;
            }
        }

        #endregion

        #region IBegin/IDrag/IEnd handlers

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // Default settings produce too coarse a drag threshold for fast
            // flicks. Override here so taps still resolve to clicks but
            // small finger drift commits to dragging.
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanAcceptDrag()) return;

            _isDragging = true;
            _axisDecided = false;
            _gestureCommitted = false;
            _dragAccumX = 0f;
            _dragAccumY = 0f;
            _appliedOffsetX = 0f;
            _velocityX = 0f;

            _lastFramePosition = eventData.position;
            _lastFrameTime = Time.unscaledTime;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            // Accumulate raw finger delta (PointerEventData.delta is per-frame).
            _dragAccumX += eventData.delta.x;
            _dragAccumY += eventData.delta.y;

            // Decide which axis owns the gesture once we've moved enough.
            if (!_axisDecided)
            {
                float absX = Mathf.Abs(_dragAccumX);
                float absY = Mathf.Abs(_dragAccumY);

                if (absX < axisLockThreshold && absY < axisLockThreshold)
                    return; // still ambiguous — don't move anything yet

                _axisDecided = true;
                _gestureCommitted = absX >= absY;

                if (!_gestureCommitted)
                {
                    // Vertical-dominant: release the gesture so any nested
                    // vertical handler (none today, but supported) can take
                    // over. We can't truly transfer mid-drag in uGUI without
                    // a custom event-system patch, so we simply ignore moves
                    // and reset state on end.
                    EndDragInternal(commit: false);
                    return;
                }

                // Do NOT re-baseline _dragAccumX: keeping the accumulated
                // delta means the sheet jumps to the finger position on the
                // first committed frame instead of lagging by the threshold.
            }

            if (!_gestureCommitted) return;

            // Velocity sampling (simple per-frame approximation).
            float now = Time.unscaledTime;
            float dt = Mathf.Max(now - _lastFrameTime, 1e-4f);
            float dx = eventData.position.x - _lastFramePosition.x;
            _velocityX = Mathf.Lerp(_velocityX, dx / dt, 0.6f); // light smoothing
            _lastFramePosition = eventData.position;
            _lastFrameTime = now;

            float requestedOffset = _dragAccumX * followRatio;
            float resolvedOffset = ApplyEdgeResistance(requestedOffset);

            StageNeighborForOffset(resolvedOffset);
            ApplyOffset(resolvedOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            EndDragInternal(commit: _gestureCommitted);
        }

        private void EndDragInternal(bool commit)
        {
            _isDragging = false;

            if (!commit)
            {
                ResetDragState();
                return;
            }

            float viewportWidth = GetViewportWidth();
            float ratio = (viewportWidth > 0f) ? (_appliedOffsetX / viewportWidth) : 0f;

            int direction = ResolveCommitDirection(ratio, _velocityX, viewportWidth);
            if (direction == 0)
            {
                StartSnap(direction: 0); // return to current
            }
            else
            {
                int target = _currentIndex + direction;
                if (!IsValidNeighbor(target))
                {
                    StartSnap(direction: 0); // can't commit past edge
                    return;
                }

                // Make sure the neighbor that snap will use is the one we
                // committed to (not the opposite side).
                StageNeighbor(target, applyOffset: true);
                StartSnap(direction);
            }
        }

        #endregion

        #region Snap

        private int ResolveCommitDirection(float positionRatio, float velocity, float viewportWidth)
        {
            // direction sign convention:
            //   positive offset = finger moved right = current sheet shifts right = neighbor enters from left
            //   so direction = -1 (move to prev/left tab) when offset is positive.
            //   direction = +1 (move to next/right tab) when offset is negative.

            // Position-based commit
            if (positionRatio >= positionThreshold) return -1;
            if (positionRatio <= -positionThreshold) return +1;

            // Velocity-based commit (flick)
            if (velocity >= velocityThreshold) return -1;
            if (velocity <= -velocityThreshold) return +1;

            return 0;
        }

        private int ResolveJumpDirection(int from, int to)
        {
            if (wrapAround)
            {
                int n = tabSheetIds.Count;
                int forward = (to - from + n) % n;
                int backward = (from - to + n) % n;
                return forward <= backward ? +1 : -1;
            }
            return to > from ? +1 : -1;
        }

        private void StartSnap(int direction, System.Action onComplete = null)
        {
            CancelSnap();
            _snapRoutine = StartCoroutine(SnapRoutine(direction, onComplete));
        }

        private System.Collections.IEnumerator SnapRoutine(int direction, System.Action onComplete)
        {
            float viewportWidth = GetViewportWidth();
            float startOffset = _appliedOffsetX;
            float targetOffset = (direction == 0) ? 0f : -direction * viewportWidth;

            float elapsed = 0f;
            float duration = Mathf.Max(snapDuration, 0.0001f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EaseUtility.Ease(t, snapEase);
                float current = Mathf.Lerp(startOffset, targetOffset, eased);

                StageNeighborForOffset(current);
                ApplyOffset(current);

                yield return null;
            }

            ApplyOffset(targetOffset);
            _snapRoutine = null;

            if (direction != 0)
            {
                int newIndex = _currentIndex + direction;
                CommitTab(newIndex);
            }
            else
            {
                // Bounced back to current — deactivate the staged neighbor.
                DeactivateNeighbor();
                ResetDragState();
            }

            onComplete?.Invoke();
        }

        private void CancelSnap()
        {
            if (_snapRoutine != null)
            {
                StopCoroutine(_snapRoutine);
                _snapRoutine = null;
            }
        }

        #endregion

        #region Sheet staging / positioning

        private void StageNeighborForOffset(float offset)
        {
            if (Mathf.Approximately(offset, 0f))
            {
                DeactivateNeighbor();
                return;
            }

            int targetIndex = (offset > 0f)
                ? _currentIndex - 1  // dragged right → previous tab enters from left
                : _currentIndex + 1; // dragged left → next tab enters from right

            StageNeighbor(targetIndex, applyOffset: false);
        }

        private void StageNeighbor(int index, bool applyOffset)
        {
            int wrapped = WrapIndex(index);
            if (!IsValidIndex(wrapped) || wrapped == _currentIndex)
            {
                DeactivateNeighbor();
                return;
            }

            if (_neighborIndex == wrapped) return; // already staged

            DeactivateNeighbor();
            _neighborIndex = wrapped;

            var sheet = GetSheet(_neighborIndex);
            if (sheet == null) return;

            var rt = (RectTransform)sheet.transform;
            sheet.gameObject.SetActive(true);
            float viewportWidth = GetViewportWidth();

            // Position the neighbor at the opposite edge of the viewport.
            float baseX = (wrapped < _currentIndex || (wrapAround && IsWrapNeighborLeft(wrapped)))
                ? -viewportWidth
                : viewportWidth;

            rt.anchoredPosition = new Vector2(baseX, rt.anchoredPosition.y);

            if (applyOffset)
            {
                rt.anchoredPosition = new Vector2(baseX + _appliedOffsetX,
                                                  rt.anchoredPosition.y);
            }
        }

        private bool IsWrapNeighborLeft(int neighborIndex)
        {
            // When wrapping, decide which physical side the neighbor enters
            // from. Going forward from last → first should still enter from
            // the right (visually next).
            int n = tabSheetIds.Count;
            if (n <= 1) return false;
            if (_currentIndex == n - 1 && neighborIndex == 0) return false;
            if (_currentIndex == 0 && neighborIndex == n - 1) return true;
            return neighborIndex < _currentIndex;
        }

        private void DeactivateNeighbor()
        {
            if (_neighborIndex < 0) return;

            // Only hide if the framework hasn't already taken it over.
            if (sheetContainer != null
                && sheetContainer.ActiveSheet != null
                && sheetContainer.ActiveSheet.Identifier != tabSheetIds[_neighborIndex])
            {
                var sheet = GetSheet(_neighborIndex);
                if (sheet != null) sheet.gameObject.SetActive(false);
            }

            _neighborIndex = -1;
        }

        private void ApplyOffset(float offset)
        {
            _appliedOffsetX = offset;

            // Move current sheet.
            var current = GetSheet(_currentIndex);
            if (current != null)
            {
                var rt = (RectTransform)current.transform;
                rt.anchoredPosition = new Vector2(offset, rt.anchoredPosition.y);
            }

            // Move neighbor sheet (if staged).
            if (_neighborIndex < 0) return;
            var neighbor = GetSheet(_neighborIndex);
            if (neighbor == null) return;

            var nrt = (RectTransform)neighbor.transform;
            float viewportWidth = GetViewportWidth();
            float baseX = IsWrapNeighborLeft(_neighborIndex) || _neighborIndex < _currentIndex
                ? -viewportWidth
                : viewportWidth;
            nrt.anchoredPosition = new Vector2(baseX + offset, nrt.anchoredPosition.y);
        }

        private float ApplyEdgeResistance(float requestedOffset)
        {
            if (wrapAround) return requestedOffset;

            // Dragged right past first tab → resistance.
            if (requestedOffset > 0f && _currentIndex == 0)
                return requestedOffset * edgeResistance;

            // Dragged left past last tab → resistance.
            if (requestedOffset < 0f && _currentIndex == tabSheetIds.Count - 1)
                return requestedOffset * edgeResistance;

            return requestedOffset;
        }

        private void CommitTab(int newIndex)
        {
            int wrapped = WrapIndex(newIndex);
            if (!IsValidIndex(wrapped))
            {
                ResetDragState();
                return;
            }

            string newId = tabSheetIds[wrapped];

            // Reset the OLD current sheet to its home position (it's about to
            // be hidden by the framework, but reset anyway so future Shows
            // start clean).
            var old = GetSheet(_currentIndex);
            if (old != null)
            {
                var rt = (RectTransform)old.transform;
                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            }

            // Pin neighbor at center (it becomes the new current).
            var neighbor = GetSheet(wrapped);
            if (neighbor != null)
            {
                var rt = (RectTransform)neighbor.transform;
                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            }

            _currentIndex = wrapped;
            _neighborIndex = -1;
            _appliedOffsetX = 0f;

            // Sync framework state without triggering its enter/exit anims.
            if (sheetContainer != null && sheetContainer.ActiveSheet != null
                && sheetContainer.ActiveSheet.Identifier != newId)
            {
                sheetContainer.Show(newId, playAnimation: false);
            }

            OnTabChanged?.Invoke(newId);
        }

        private void ApplyTabImmediate(int targetIndex)
        {
            int wrapped = WrapIndex(targetIndex);
            if (!IsValidIndex(wrapped) || wrapped == _currentIndex) return;

            DeactivateNeighbor();

            var old = GetSheet(_currentIndex);
            if (old != null)
            {
                var rt = (RectTransform)old.transform;
                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            }

            _currentIndex = wrapped;
            _appliedOffsetX = 0f;

            var nu = GetSheet(_currentIndex);
            if (nu != null)
            {
                var rt = (RectTransform)nu.transform;
                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            }

            if (sheetContainer != null)
            {
                sheetContainer.Show(tabSheetIds[_currentIndex], playAnimation: false);
            }

            OnTabChanged?.Invoke(tabSheetIds[_currentIndex]);
        }

        private void ResetDragState()
        {
            DeactivateNeighbor();
            _appliedOffsetX = 0f;

            var current = GetSheet(_currentIndex);
            if (current != null)
            {
                var rt = (RectTransform)current.transform;
                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            }
        }

        #endregion

        #region Helpers

        private bool CanAcceptDrag()
        {
            if (sheetContainer == null) return false;
            if (tabSheetIds == null || tabSheetIds.Count < 2) return false;
            if (disableDuringFrameworkTransition && sheetContainer.IsInTransition) return false;
            if (sheetContainer.ActiveSheet == null) return false;

            // Keep our index in sync if something else changed the active sheet.
            string activeId = sheetContainer.ActiveSheet.Identifier;
            int idx = tabSheetIds.IndexOf(activeId);
            if (idx >= 0 && idx != _currentIndex) _currentIndex = idx;

            return _currentIndex >= 0;
        }

        private Sheet GetSheet(int index)
        {
            if (!IsValidIndex(index)) return null;
            if (sheetContainer == null) return null;
            return sheetContainer.GetSheet(tabSheetIds[index]);
        }

        private bool IsValidIndex(int index)
            => index >= 0 && index < tabSheetIds.Count;

        private bool IsValidNeighbor(int index)
            => wrapAround
                ? IsValidIndex(WrapIndex(index))
                : IsValidIndex(index);

        private int WrapIndex(int index)
        {
            int n = tabSheetIds.Count;
            if (n <= 0) return -1;
            if (!wrapAround) return index;
            return ((index % n) + n) % n;
        }

        private float GetViewportWidth()
        {
            if (_viewport == null) _viewport = (RectTransform)transform;
            float w = _viewport.rect.width;
            // Fully qualify: EnhancedUI.Screen (the framework's Sheet base class)
            // shadows UnityEngine.Screen when `using EnhancedUI;` is in scope.
            return w > 0f ? w : UnityEngine.Screen.width;
        }

        private void EnsureRaycastTarget()
        {
            _raycastTarget = GetComponent<Graphic>();
            if (_raycastTarget != null)
            {
                _raycastTarget.raycastTarget = true;
                return;
            }

            // Add a fully transparent Image so EventSystem raycasts hit us.
            var image = gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
            _raycastTarget = image;
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (sheetContainer == null)
            {
                sheetContainer = GetComponentInParent<SheetContainer>();
                if (sheetContainer == null)
                    sheetContainer = GetComponentInChildren<SheetContainer>();
            }

            // Clamp safety
            if (snapDuration < 0.01f) snapDuration = 0.01f;
            if (velocityThreshold < 0f) velocityThreshold = 0f;
            if (axisLockThreshold < 0f) axisLockThreshold = 0f;
        }

        [ContextMenu("Debug: Print State")]
        private void DebugPrintState()
        {
            if (!Application.isPlaying) return;
            Debug.Log($"[SheetSwipePager] current={_currentIndex} " +
                      $"({(IsValidIndex(_currentIndex) ? tabSheetIds[_currentIndex] : "n/a")}) " +
                      $"neighbor={_neighborIndex} " +
                      $"dragging={_isDragging} snapping={IsSnapping} " +
                      $"offset={_appliedOffsetX:F1}px");
        }
#endif
    }
}
