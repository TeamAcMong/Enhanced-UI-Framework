using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace EnhancedUI
{
    /// <summary>
    /// Backdrop for modal dialogs
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ModalBackdrop : MonoBehaviour, IPointerClickHandler
    {
        private CanvasGroup _canvasGroup;
        private Action _onClicked;

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        public float Alpha
        {
            get => CanvasGroup.alpha;
            set => CanvasGroup.alpha = value;
        }

        public Action OnClicked
        {
            get => _onClicked;
            set => _onClicked = value;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke();
        }

        /// <summary>
        /// Create a backdrop GameObject
        /// </summary>
        public static ModalBackdrop Create(Transform parent, Color color, bool closeOnClick = false)
        {
            var go = new GameObject("[Modal Backdrop]");
            go.layer = parent.gameObject.layer;

            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = closeOnClick;

            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            var backdrop = go.AddComponent<ModalBackdrop>();

            return backdrop;
        }
    }

    /// <summary>
    /// Strategy for managing modal backdrops
    /// </summary>
    public enum ModalBackdropStrategy
    {
        /// <summary>
        /// Generate a backdrop per modal
        /// </summary>
        GeneratePerModal,

        /// <summary>
        /// Only the first modal has a backdrop
        /// </summary>
        OnlyFirstBackdrop,

        /// <summary>
        /// Change backdrop order before animation
        /// </summary>
        ChangeOrderBeforeAnimation,

        /// <summary>
        /// Change backdrop order after animation
        /// </summary>
        ChangeOrderAfterAnimation
    }
}
