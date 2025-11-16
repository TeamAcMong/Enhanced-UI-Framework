using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EnhancedUI.Demo.Components
{
    /// <summary>
    /// Bottom navigation tab bar - common mobile UI pattern
    /// Allows navigation between main sections (Home, Shop, Battle, etc.)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BottomNavigation : MonoBehaviour
    {
        [Header("Tab Configuration")]
        [SerializeField] private List<NavigationTab> tabs = new List<NavigationTab>();
        [SerializeField] private int defaultTabIndex = 0;

        [Header("Visual Settings")]
        [SerializeField] private Color selectedColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        [SerializeField] private float selectedScale = 1.1f;
        [SerializeField] private float unselectedScale = 1f;
        [SerializeField] private bool animateTransition = true;
        [SerializeField] private float transitionDuration = 0.2f;

        private int _currentTabIndex = -1;
        private bool _isTransitioning = false;

        // Events
        public event Action<int, string> OnTabChanged; // index, tabId

        private void Start()
        {
            // Setup all tabs
            SetupTabs();

            // Select default tab
            SelectTab(defaultTabIndex, false);
        }

        /// <summary>
        /// Setup tab buttons and listeners
        /// </summary>
        private void SetupTabs()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].button == null) continue;

                int index = i; // Capture for closure
                tabs[i].button.onClick.AddListener(() => OnTabClicked(index));

                // Initialize visual state
                UpdateTabVisual(i, false, false);
            }
        }

        /// <summary>
        /// Select a tab by index
        /// </summary>
        public void SelectTab(int index, bool notifyListeners = true)
        {
            if (index < 0 || index >= tabs.Count)
            {
                Debug.LogWarning($"[BottomNavigation] Invalid tab index: {index}");
                return;
            }

            if (index == _currentTabIndex)
            {
                Debug.Log($"[BottomNavigation] Tab {index} already selected");
                return;
            }

            if (_isTransitioning)
            {
                Debug.Log("[BottomNavigation] Transition in progress, ignoring selection");
                return;
            }

            // Update previous tab
            if (_currentTabIndex >= 0 && _currentTabIndex < tabs.Count)
            {
                UpdateTabVisual(_currentTabIndex, false, animateTransition);
            }

            // Update current tab
            _currentTabIndex = index;
            UpdateTabVisual(_currentTabIndex, true, animateTransition);

            // Notify listeners
            if (notifyListeners)
            {
                OnTabChanged?.Invoke(_currentTabIndex, tabs[_currentTabIndex].tabId);
            }

            Debug.Log($"[BottomNavigation] Selected tab: {tabs[_currentTabIndex].tabId}");
        }

        /// <summary>
        /// Select a tab by ID
        /// </summary>
        public void SelectTab(string tabId, bool notifyListeners = true)
        {
            int index = tabs.FindIndex(t => t.tabId == tabId);
            if (index >= 0)
            {
                SelectTab(index, notifyListeners);
            }
            else
            {
                Debug.LogWarning($"[BottomNavigation] Tab not found: {tabId}");
            }
        }

        /// <summary>
        /// Get currently selected tab index
        /// </summary>
        public int GetCurrentTabIndex()
        {
            return _currentTabIndex;
        }

        /// <summary>
        /// Get currently selected tab ID
        /// </summary>
        public string GetCurrentTabId()
        {
            if (_currentTabIndex >= 0 && _currentTabIndex < tabs.Count)
            {
                return tabs[_currentTabIndex].tabId;
            }
            return null;
        }

        /// <summary>
        /// Show notification badge on a tab
        /// </summary>
        public void SetTabNotification(int index, bool show, int count = 0)
        {
            if (index < 0 || index >= tabs.Count) return;

            var tab = tabs[index];
            if (tab.notificationBadge != null)
            {
                tab.notificationBadge.SetActive(show);

                // Update count text if available
                if (tab.notificationCountText != null && count > 0)
                {
                    tab.notificationCountText.text = count > 99 ? "99+" : count.ToString();
                }
            }
        }

        /// <summary>
        /// Show notification badge on a tab by ID
        /// </summary>
        public void SetTabNotification(string tabId, bool show, int count = 0)
        {
            int index = tabs.FindIndex(t => t.tabId == tabId);
            if (index >= 0)
            {
                SetTabNotification(index, show, count);
            }
        }

        /// <summary>
        /// Update visual state of a tab
        /// </summary>
        private void UpdateTabVisual(int index, bool selected, bool animate)
        {
            if (index < 0 || index >= tabs.Count) return;

            var tab = tabs[index];
            Color targetColor = selected ? selectedColor : unselectedColor;
            float targetScale = selected ? selectedScale : unselectedScale;

            if (animate)
            {
                // Animate color and scale
                StartCoroutine(AnimateTab(tab, targetColor, targetScale));
            }
            else
            {
                // Immediate update
                ApplyTabVisual(tab, targetColor, targetScale);
            }
        }

        /// <summary>
        /// Apply visual state to tab immediately
        /// </summary>
        private void ApplyTabVisual(NavigationTab tab, Color color, float scale)
        {
            if (tab.icon != null)
            {
                tab.icon.color = color;
                tab.icon.transform.localScale = Vector3.one * scale;
            }

            if (tab.label != null)
            {
                tab.label.color = color;
            }
        }

        /// <summary>
        /// Animate tab transition
        /// </summary>
        private System.Collections.IEnumerator AnimateTab(NavigationTab tab, Color targetColor, float targetScale)
        {
            _isTransitioning = true;

            float elapsed = 0f;
            Color startColor = tab.icon != null ? tab.icon.color : Color.white;
            float startScale = tab.icon != null ? tab.icon.transform.localScale.x : 1f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / transitionDuration);

                // Ease out
                float eased = 1f - Mathf.Pow(1f - progress, 3f);

                Color currentColor = Color.Lerp(startColor, targetColor, eased);
                float currentScale = Mathf.Lerp(startScale, targetScale, eased);

                ApplyTabVisual(tab, currentColor, currentScale);

                yield return null;
            }

            // Ensure final state
            ApplyTabVisual(tab, targetColor, targetScale);

            _isTransitioning = false;
        }

        /// <summary>
        /// Tab button clicked handler
        /// </summary>
        private void OnTabClicked(int index)
        {
            Debug.Log($"[BottomNavigation] Tab {index} clicked");
            SelectTab(index, true);
        }

        /// <summary>
        /// Enable or disable all tab interactions
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            foreach (var tab in tabs)
            {
                if (tab.button != null)
                {
                    tab.button.interactable = interactable;
                }
            }
        }

#if UNITY_EDITOR
        // Editor-only: Validate tab configuration
        private void OnValidate()
        {
            if (tabs.Count == 0)
            {
                Debug.LogWarning($"[BottomNavigation] No tabs configured on {gameObject.name}", this);
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                if (string.IsNullOrEmpty(tabs[i].tabId))
                {
                    Debug.LogWarning($"[BottomNavigation] Tab {i} has no ID on {gameObject.name}", this);
                }

                if (tabs[i].button == null)
                {
                    Debug.LogWarning($"[BottomNavigation] Tab {i} ({tabs[i].tabId}) has no button reference on {gameObject.name}", this);
                }
            }
        }
#endif
    }

    /// <summary>
    /// Navigation tab configuration
    /// </summary>
    [Serializable]
    public class NavigationTab
    {
        [Tooltip("Unique identifier for this tab")]
        public string tabId;

        [Tooltip("Display name for the tab")]
        public string displayName;

        [Tooltip("Button component for this tab")]
        public Button button;

        [Tooltip("Icon image for this tab")]
        public Image icon;

        [Tooltip("Label text for this tab")]
        public TextMeshProUGUI label;

        [Tooltip("Optional notification badge")]
        public GameObject notificationBadge;

        [Tooltip("Optional notification count text")]
        public TextMeshProUGUI notificationCountText;
    }
}
