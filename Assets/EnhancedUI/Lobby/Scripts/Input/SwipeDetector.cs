using UnityEngine;
using EnhancedUI;
using System.Collections.Generic;

namespace EnhancedUI.Demo.Input
{
    /// <summary>
    /// Swipe gesture detector for horizontal tab navigation
    /// Detects left/right swipes and switches between tabs
    /// </summary>
    public class SwipeDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SheetContainer sheetContainer;

        [Header("Swipe Settings")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float maxSwipeTime = 1f;

        [Header("Tab Configuration")]
        [SerializeField] private List<string> tabSheetIds = new List<string>
        {
            "HomeSheet",
            "BattleSheet",
            "InventorySheet",
            "ShopSheet"
        };

        private Vector2 _touchStart;
        private float _touchStartTime;
        private bool _isSwiping;

        private void Update()
        {
            // Handle touch input (mobile)
            if (UnityEngine.Input.touchCount > 0)
            {
                HandleTouch(UnityEngine.Input.GetTouch(0));
            }
            // Handle mouse input (editor/standalone)
            else if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                StartSwipe(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && _isSwiping)
            {
                EndSwipe(UnityEngine.Input.mousePosition);
            }
        }

        private void HandleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartSwipe(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (_isSwiping)
                    {
                        EndSwipe(touch.position);
                    }
                    break;
            }
        }

        private void StartSwipe(Vector2 position)
        {
            _touchStart = position;
            _touchStartTime = Time.time;
            _isSwiping = true;
        }

        private void EndSwipe(Vector2 position)
        {
            if (!_isSwiping)
                return;

            _isSwiping = false;

            // Calculate swipe delta and time
            Vector2 swipeDelta = position - _touchStart;
            float swipeTime = Time.time - _touchStartTime;

            // Check if swipe is valid
            if (swipeTime > maxSwipeTime)
            {
                Debug.Log("[SwipeDetector] Swipe too slow, ignoring");
                return;
            }

            if (Mathf.Abs(swipeDelta.x) < swipeThreshold)
            {
                Debug.Log("[SwipeDetector] Swipe distance too short, ignoring");
                return;
            }

            // Determine swipe direction
            if (swipeDelta.x > 0)
            {
                OnSwipeRight();
            }
            else
            {
                OnSwipeLeft();
            }
        }

        private void OnSwipeLeft()
        {
            Debug.Log("[SwipeDetector] Swipe left detected");

            if (sheetContainer == null || sheetContainer.IsInTransition)
            {
                Debug.Log("[SwipeDetector] Cannot switch - container null or in transition");
                return;
            }

            // Move to next tab (right)
            int currentIndex = GetCurrentTabIndex();
            int nextIndex = Mathf.Min(currentIndex + 1, tabSheetIds.Count - 1);

            if (nextIndex != currentIndex)
            {
                Debug.Log($"[SwipeDetector] Switching from tab {currentIndex} to {nextIndex}");
                sheetContainer.Show(tabSheetIds[nextIndex], playAnimation: true);
            }
            else
            {
                Debug.Log("[SwipeDetector] Already at rightmost tab");
            }
        }

        private void OnSwipeRight()
        {
            Debug.Log("[SwipeDetector] Swipe right detected");

            if (sheetContainer == null || sheetContainer.IsInTransition)
            {
                Debug.Log("[SwipeDetector] Cannot switch - container null or in transition");
                return;
            }

            // Move to previous tab (left)
            int currentIndex = GetCurrentTabIndex();
            int prevIndex = Mathf.Max(currentIndex - 1, 0);

            if (prevIndex != currentIndex)
            {
                Debug.Log($"[SwipeDetector] Switching from tab {currentIndex} to {prevIndex}");
                sheetContainer.Show(tabSheetIds[prevIndex], playAnimation: true);
            }
            else
            {
                Debug.Log("[SwipeDetector] Already at leftmost tab");
            }
        }

        private int GetCurrentTabIndex()
        {
            if (sheetContainer == null || string.IsNullOrEmpty(sheetContainer.ActiveSheet.Identifier)) 
                return 0;

            int index = tabSheetIds.IndexOf(sheetContainer.ActiveSheet.Identifier);
            return index >= 0 ? index : 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // Auto-find SheetContainer if not assigned
            if (sheetContainer == null)
            {
                sheetContainer = GetComponentInChildren<SheetContainer>();
                if (sheetContainer != null)
                {
                    Debug.Log("[SwipeDetector] Auto-found SheetContainer");
                }
            }
        }

        [ContextMenu("Print Current Tab")]
        private void PrintCurrentTab()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SwipeDetector] Only works in Play Mode");
                return;
            }

            int index = GetCurrentTabIndex();
            Debug.Log($"[SwipeDetector] Current tab: {index} ({(index < tabSheetIds.Count ? tabSheetIds[index] : "Unknown")})");
        }
#endif
    }
}
