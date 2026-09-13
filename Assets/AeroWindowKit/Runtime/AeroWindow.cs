using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BavoDev.AeroWindowKit
{
    /// <summary>
    /// Runtime controller for a desktop-style uGUI window.
    /// Supports focus, minimize, maximize, snapping, resizing, and optional layout persistence.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class AeroWindow : MonoBehaviour, IPointerDownHandler
    {
        public enum CloseMode
        {
            Hide,
            Destroy
        }

        public enum SnapState
        {
            None,
            Left,
            Right
        }

        [Serializable]
        private sealed class LayoutData
        {
            public int version = 1;
            public float anchorMinX;
            public float anchorMinY;
            public float anchorMaxX;
            public float anchorMaxY;
            public float pivotX;
            public float pivotY;
            public float positionX;
            public float positionY;
            public float sizeX;
            public float sizeY;
            public int mode;
        }

        [Header("Identity")]
        [SerializeField] private string windowTitle = "Window";

        [Header("Behavior")]
        [SerializeField] private CloseMode closeMode = CloseMode.Hide;
        [SerializeField] private bool keepInsideCanvas = true;
        [SerializeField] private bool allowMaximize = true;

        [Header("Resize")]
        [SerializeField] private bool allowResize = true;
        [SerializeField] private Vector2 minimumSize = new Vector2(280f, 160f);

        [Header("Snap")]
        [SerializeField] private bool allowSnap = true;
        [Tooltip("Distance from a screen edge, in screen pixels, that triggers snapping when a drag ends.")]
        [SerializeField] private float snapDistance = 36f;

        [Header("Layout persistence")]
        [SerializeField] private bool rememberLayout = false;
        [Tooltip("Optional stable ID. Leave blank to derive one from the scene hierarchy.")]
        [SerializeField] private string persistenceId = "";
        [SerializeField] private bool savePlayerPrefsImmediately = false;

        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private RectTransform canvasRect;
        private AeroWindowManager manager;
        private bool isMinimized;
        private bool isMaximized;
        private SnapState snapState;

        private bool hasRestoreRect;
        private Vector2 restoreAnchorMin;
        private Vector2 restoreAnchorMax;
        private Vector2 restorePivot;
        private Vector2 restoreAnchoredPosition;
        private Vector2 restoreSizeDelta;

        public string WindowTitle
        {
            get => string.IsNullOrWhiteSpace(windowTitle) ? name : windowTitle;
            set => windowTitle = value;
        }

        public bool IsMinimized => isMinimized;
        public bool IsMaximized => isMaximized;
        public bool IsSnapped => snapState != SnapState.None;
        public SnapState CurrentSnapState => snapState;
        public bool AllowResize => allowResize;
        public RectTransform RectTransform => rectTransform;
        public Canvas RootCanvas => rootCanvas;

        private bool IsSpecialLayout => isMaximized || snapState != SnapState.None;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            RefreshCanvasReferences();

            manager = AeroWindowManager.FindFor(this);
            if (manager != null)
                manager.Register(this);
        }

        private void Start()
        {
            if (rememberLayout)
                LoadLayout();
        }

        private void OnValidate()
        {
            minimumSize.x = Mathf.Max(1f, minimumSize.x);
            minimumSize.y = Mathf.Max(1f, minimumSize.y);
            snapDistance = Mathf.Max(0f, snapDistance);
        }

        private void OnEnable()
        {
            if (rectTransform == null)
                rectTransform = (RectTransform)transform;

            RefreshCanvasReferences();

            if (manager == null)
                manager = AeroWindowManager.FindFor(this);

            if (manager != null)
                manager.Register(this);
        }

        private void OnDisable()
        {
            if (rememberLayout)
                SaveLayout();
        }

        private void OnDestroy()
        {
            if (manager != null)
                manager.Unregister(this);
        }

        private void OnApplicationQuit()
        {
            if (rememberLayout)
                SaveLayout();
        }

        private void RefreshCanvasReferences()
        {
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null)
                rootCanvas = rootCanvas.rootCanvas;

            canvasRect = rootCanvas != null ? rootCanvas.transform as RectTransform : null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Focus();
        }

        public void Focus()
        {
            if (manager != null)
                manager.Focus(this);
            else
                transform.SetAsLastSibling();
        }

        public void Show()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            isMinimized = false;

            if (manager == null)
                manager = AeroWindowManager.FindFor(this);
            if (manager != null)
                manager.NotifyRestored(this);

            Focus();
        }

        public void Minimize()
        {
            if (isMinimized)
                return;

            if (rememberLayout)
                SaveLayout();

            isMinimized = true;
            if (manager != null)
                manager.NotifyMinimized(this);

            gameObject.SetActive(false);
        }

        public void RestoreFromTaskbar()
        {
            isMinimized = false;
            gameObject.SetActive(true);

            if (manager == null)
                manager = AeroWindowManager.FindFor(this);
            if (manager != null)
                manager.NotifyRestored(this);

            Focus();
        }

        public void ToggleMaximize()
        {
            if (!allowMaximize)
                return;

            if (isMaximized)
                RestoreSize();
            else
                Maximize();
        }

        public void Maximize()
        {
            if (!allowMaximize || isMaximized)
                return;

            PrepareRestoreRect();
            ApplyMaximizedRect();
            SaveLayoutIfEnabled();
            Focus();
        }

        public void SnapLeft()
        {
            SnapTo(SnapState.Left);
        }

        public void SnapRight()
        {
            SnapTo(SnapState.Right);
        }

        public void RestoreSize()
        {
            if (!IsSpecialLayout || !hasRestoreRect)
                return;

            ApplyRestoreRect();
            SaveLayoutIfEnabled();
            ClampInsideCanvas();
            Focus();
        }

        public void Close()
        {
            SaveLayoutIfEnabled();

            if (manager != null)
                manager.Unregister(this);

            if (closeMode == CloseMode.Destroy)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }

        /// <summary>
        /// Called by a title bar before a drag starts. Restores snapped/maximized windows
        /// so the user can immediately drag them again.
        /// </summary>
        public void BeginMove()
        {
            Focus();

            if (IsSpecialLayout && hasRestoreRect)
                ApplyRestoreRect();
        }

        public void MoveByScreenDelta(Vector2 screenDelta)
        {
            if (IsSpecialLayout)
                return;

            rectTransform.anchoredPosition += ScreenDeltaToCanvasDelta(screenDelta);
            ClampInsideCanvas();
        }

        /// <summary>
        /// Called by a title bar when dragging ends. Snaps to an edge when applicable.
        /// </summary>
        public void EndMove(Vector2 pointerScreenPosition)
        {
            if (!TrySnap(pointerScreenPosition))
                ClampInsideCanvas();

            SaveLayoutIfEnabled();
        }

        public void BeginResize()
        {
            if (!allowResize)
                return;

            Focus();

            if (IsSpecialLayout && hasRestoreRect)
                ApplyRestoreRect();
        }

        public void ResizeByScreenDelta(Vector2 screenDelta, AeroResizeEdges edges)
        {
            if (!allowResize || rectTransform == null || edges == AeroResizeEdges.None || IsSpecialLayout)
                return;

            Vector2 delta = ScreenDeltaToCanvasDelta(screenDelta);
            Vector2 oldSize = rectTransform.rect.size;
            Vector2 newSize = oldSize;
            Vector2 positionDelta = Vector2.zero;

            if ((edges & AeroResizeEdges.Right) != 0)
            {
                float requested = oldSize.x + delta.x;
                newSize.x = Mathf.Max(minimumSize.x, requested);
                float widthDelta = newSize.x - oldSize.x;
                positionDelta.x += rectTransform.pivot.x * widthDelta;
            }
            else if ((edges & AeroResizeEdges.Left) != 0)
            {
                float requested = oldSize.x - delta.x;
                newSize.x = Mathf.Max(minimumSize.x, requested);
                float widthDelta = newSize.x - oldSize.x;
                positionDelta.x -= (1f - rectTransform.pivot.x) * widthDelta;
            }

            if ((edges & AeroResizeEdges.Top) != 0)
            {
                float requested = oldSize.y + delta.y;
                newSize.y = Mathf.Max(minimumSize.y, requested);
                float heightDelta = newSize.y - oldSize.y;
                positionDelta.y += rectTransform.pivot.y * heightDelta;
            }
            else if ((edges & AeroResizeEdges.Bottom) != 0)
            {
                float requested = oldSize.y - delta.y;
                newSize.y = Mathf.Max(minimumSize.y, requested);
                float heightDelta = newSize.y - oldSize.y;
                positionDelta.y -= (1f - rectTransform.pivot.y) * heightDelta;
            }

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
            rectTransform.anchoredPosition += positionDelta;
            ClampInsideCanvas();
        }

        public void EndResize()
        {
            ClampInsideCanvas();
            SaveLayoutIfEnabled();
        }

        public bool TrySnap(Vector2 pointerScreenPosition)
        {
            if (!allowSnap || canvasRect == null || rootCanvas == null)
                return false;

            Vector3[] corners = new Vector3[4];
            canvasRect.GetWorldCorners(corners);

            Camera camera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);

            if (allowMaximize && pointerScreenPosition.y >= topRight.y - snapDistance)
            {
                Maximize();
                return true;
            }

            if (pointerScreenPosition.x <= bottomLeft.x + snapDistance)
            {
                SnapLeft();
                return true;
            }

            if (pointerScreenPosition.x >= topRight.x - snapDistance)
            {
                SnapRight();
                return true;
            }

            return false;
        }

        public void ClampInsideCanvas()
        {
            if (!keepInsideCanvas || rectTransform == null || canvasRect == null || !gameObject.activeInHierarchy)
                return;

            Vector3[] windowCorners = new Vector3[4];
            Vector3[] canvasCorners = new Vector3[4];
            rectTransform.GetWorldCorners(windowCorners);
            canvasRect.GetWorldCorners(canvasCorners);

            float dx = 0f;
            float dy = 0f;

            if (windowCorners[0].x < canvasCorners[0].x)
                dx = canvasCorners[0].x - windowCorners[0].x;
            else if (windowCorners[2].x > canvasCorners[2].x)
                dx = canvasCorners[2].x - windowCorners[2].x;

            if (windowCorners[0].y < canvasCorners[0].y)
                dy = canvasCorners[0].y - windowCorners[0].y;
            else if (windowCorners[2].y > canvasCorners[2].y)
                dy = canvasCorners[2].y - windowCorners[2].y;

            if (Mathf.Abs(dx) > Mathf.Epsilon || Mathf.Abs(dy) > Mathf.Epsilon)
                rectTransform.position += new Vector3(dx, dy, 0f);
        }

        public void SaveLayout()
        {
            if (!rememberLayout || rectTransform == null)
                return;

            LayoutData data = new LayoutData();
            Vector2 anchorMin;
            Vector2 anchorMax;
            Vector2 pivot;
            Vector2 position;
            Vector2 size;

            if (IsSpecialLayout && hasRestoreRect)
            {
                anchorMin = restoreAnchorMin;
                anchorMax = restoreAnchorMax;
                pivot = restorePivot;
                position = restoreAnchoredPosition;
                size = restoreSizeDelta;
            }
            else
            {
                anchorMin = rectTransform.anchorMin;
                anchorMax = rectTransform.anchorMax;
                pivot = rectTransform.pivot;
                position = rectTransform.anchoredPosition;
                size = rectTransform.sizeDelta;
            }

            data.anchorMinX = anchorMin.x;
            data.anchorMinY = anchorMin.y;
            data.anchorMaxX = anchorMax.x;
            data.anchorMaxY = anchorMax.y;
            data.pivotX = pivot.x;
            data.pivotY = pivot.y;
            data.positionX = position.x;
            data.positionY = position.y;
            data.sizeX = size.x;
            data.sizeY = size.y;
            data.mode = isMaximized ? 1 : snapState == SnapState.Left ? 2 : snapState == SnapState.Right ? 3 : 0;

            PlayerPrefs.SetString(GetPersistenceKey(), JsonUtility.ToJson(data));
            if (savePlayerPrefsImmediately)
                PlayerPrefs.Save();
        }

        public bool LoadLayout()
        {
            if (!rememberLayout || rectTransform == null)
                return false;

            string key = GetPersistenceKey();
            if (!PlayerPrefs.HasKey(key))
                return false;

            LayoutData data;
            try
            {
                data = JsonUtility.FromJson<LayoutData>(PlayerPrefs.GetString(key));
            }
            catch
            {
                return false;
            }

            if (data == null || data.version != 1)
                return false;

            restoreAnchorMin = new Vector2(data.anchorMinX, data.anchorMinY);
            restoreAnchorMax = new Vector2(data.anchorMaxX, data.anchorMaxY);
            restorePivot = new Vector2(data.pivotX, data.pivotY);
            restoreAnchoredPosition = new Vector2(data.positionX, data.positionY);
            restoreSizeDelta = new Vector2(data.sizeX, data.sizeY);
            hasRestoreRect = true;

            ApplyRestoreRect();

            if (data.mode == 1 && allowMaximize)
                ApplyMaximizedRect();
            else if (data.mode == 2 && allowSnap)
                ApplySnapRect(SnapState.Left);
            else if (data.mode == 3 && allowSnap)
                ApplySnapRect(SnapState.Right);
            else
                ClampInsideCanvas();

            return true;
        }

        public void ResetSavedLayout()
        {
            if (!rememberLayout)
                return;

            PlayerPrefs.DeleteKey(GetPersistenceKey());
            if (savePlayerPrefsImmediately)
                PlayerPrefs.Save();
        }

        private void SnapTo(SnapState state)
        {
            if (!allowSnap || state == SnapState.None)
                return;

            if (snapState == state && !isMaximized)
                return;

            PrepareRestoreRect();
            ApplySnapRect(state);
            SaveLayoutIfEnabled();
            Focus();
        }

        private void PrepareRestoreRect()
        {
            if (IsSpecialLayout && hasRestoreRect)
                return;

            restoreAnchorMin = rectTransform.anchorMin;
            restoreAnchorMax = rectTransform.anchorMax;
            restorePivot = rectTransform.pivot;
            restoreAnchoredPosition = rectTransform.anchoredPosition;
            restoreSizeDelta = rectTransform.sizeDelta;
            hasRestoreRect = true;
        }

        private void ApplyRestoreRect()
        {
            if (!hasRestoreRect)
                return;

            rectTransform.anchorMin = restoreAnchorMin;
            rectTransform.anchorMax = restoreAnchorMax;
            rectTransform.pivot = restorePivot;
            rectTransform.anchoredPosition = restoreAnchoredPosition;
            rectTransform.sizeDelta = restoreSizeDelta;
            isMaximized = false;
            snapState = SnapState.None;
        }

        private void ApplyMaximizedRect()
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            isMaximized = true;
            snapState = SnapState.None;
        }

        private void ApplySnapRect(SnapState state)
        {
            if (state == SnapState.Left)
            {
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
            }

            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            isMaximized = false;
            snapState = state;
        }

        private Vector2 ScreenDeltaToCanvasDelta(Vector2 screenDelta)
        {
            if (rootCanvas != null && rootCanvas.scaleFactor > 0f)
                return screenDelta / rootCanvas.scaleFactor;

            return screenDelta;
        }

        private void SaveLayoutIfEnabled()
        {
            if (rememberLayout)
                SaveLayout();
        }

        private string GetPersistenceKey()
        {
            string id = string.IsNullOrWhiteSpace(persistenceId) ? BuildHierarchyId() : persistenceId.Trim();
            return "AeroWindowKit.Layout." + id;
        }

        private string BuildHierarchyId()
        {
            StringBuilder builder = new StringBuilder();
            Transform current = transform;

            while (current != null)
            {
                if (builder.Length > 0)
                    builder.Insert(0, "/");
                builder.Insert(0, current.name);
                current = current.parent;
            }

            string sceneName = gameObject.scene.IsValid() ? gameObject.scene.name : "Scene";
            return sceneName + ":" + builder;
        }
    }
}
