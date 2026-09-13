using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BavoDev.AeroWindowKit
{
    [Flags]
    public enum AeroResizeEdges
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Top = 1 << 2,
        Bottom = 1 << 3
    }

    /// <summary>
    /// Add this to a UI element placed on a window edge or corner.
    /// By default it behaves as a bottom-right resize grip.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AeroWindowResizeHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private AeroWindow targetWindow;
        [SerializeField] private AeroResizeEdges edges = AeroResizeEdges.Right | AeroResizeEdges.Bottom;

        public AeroResizeEdges Edges
        {
            get => edges;
            set => edges = value;
        }

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
                targetWindow.BeginResize();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (targetWindow != null)
                targetWindow.ResizeByScreenDelta(eventData.delta, edges);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (targetWindow != null)
                targetWindow.EndResize();
        }
    }
}
