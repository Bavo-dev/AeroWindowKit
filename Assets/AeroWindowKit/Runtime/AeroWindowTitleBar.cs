using UnityEngine;
using UnityEngine.EventSystems;

namespace BavoDev.AeroWindowKit
{
    /// <summary>
    /// Attach this to a window title bar to make its AeroWindow draggable.
    /// Dragging to the top/left/right edge can maximize or snap the window.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AeroWindowTitleBar : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerClickHandler
    {
        [SerializeField] private AeroWindow targetWindow;
        [SerializeField] private bool doubleClickToMaximize = true;

        private void Awake()
        {
            if (targetWindow == null)
                targetWindow = GetComponentInParent<AeroWindow>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (targetWindow != null)
                targetWindow.Focus();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (targetWindow != null)
                targetWindow.BeginMove();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (targetWindow != null)
                targetWindow.MoveByScreenDelta(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (targetWindow != null)
                targetWindow.EndMove(eventData.position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (doubleClickToMaximize && eventData.clickCount == 2 && targetWindow != null)
                targetWindow.ToggleMaximize();
        }
    }
}
