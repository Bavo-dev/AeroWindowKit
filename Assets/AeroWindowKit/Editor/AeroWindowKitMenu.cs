using BavoDev.AeroWindowKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BavoDev.AeroWindowKit.Editor
{
    public static class AeroWindowKitMenu
    {
        [MenuItem("Tools/AeroWindowKit/Create Demo UI")]
        public static void CreateDemoUI()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
                canvas = CreateCanvas();

            EnsureEventSystem();

            GameObject desktop = CreateUIObject("Aero Desktop", canvas.transform, typeof(Image));
            RectTransform desktopRect = desktop.GetComponent<RectTransform>();
            Stretch(desktopRect);
            desktop.GetComponent<Image>().color = new Color(0.50f, 0.82f, 0.98f, 1f);

            GameObject taskbarObject = CreateUIObject("Taskbar", desktop.transform, typeof(Image), typeof(HorizontalLayoutGroup), typeof(AeroTaskbar));
            RectTransform taskbarRect = taskbarObject.GetComponent<RectTransform>();
            taskbarRect.anchorMin = new Vector2(0f, 0f);
            taskbarRect.anchorMax = new Vector2(1f, 0f);
            taskbarRect.pivot = new Vector2(0.5f, 0f);
            taskbarRect.sizeDelta = new Vector2(0f, 48f);
            taskbarRect.anchoredPosition = Vector2.zero;
            taskbarObject.GetComponent<Image>().color = new Color(0.10f, 0.35f, 0.58f, 0.96f);
            HorizontalLayoutGroup layout = taskbarObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 7, 7);
            layout.spacing = 6f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            GameObject managerObject = new GameObject("AeroWindowManager", typeof(RectTransform), typeof(AeroWindowManager));
            Undo.RegisterCreatedObjectUndo(managerObject, "Create AeroWindowKit Demo");
            managerObject.transform.SetParent(desktop.transform, false);

            GameObject windowObject = CreateUIObject("Welcome Window", desktop.transform, typeof(Image), typeof(AeroWindow));
            RectTransform windowRect = windowObject.GetComponent<RectTransform>();
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(620f, 380f);
            windowRect.anchoredPosition = new Vector2(0f, 35f);
            windowObject.GetComponent<Image>().color = new Color(0.94f, 0.98f, 1f, 0.98f);
            AeroWindow window = windowObject.GetComponent<AeroWindow>();
            window.WindowTitle = "AeroWindowKit Demo";

            GameObject titleBar = CreateUIObject("Title Bar", windowObject.transform, typeof(Image), typeof(AeroWindowTitleBar));
            RectTransform titleRect = titleBar.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 44f);
            titleRect.anchoredPosition = Vector2.zero;
            titleBar.GetComponent<Image>().color = new Color(0.08f, 0.48f, 0.78f, 1f);

            CreateText("Title", titleBar.transform, "AeroWindowKit Demo", 18, TextAnchor.MiddleLeft, new Vector2(12f, 0f), new Vector2(-156f, 0f));

            Button minButton = CreateButton("Minimize", titleBar.transform, "—", new Vector2(-120f, -4f));
            Button maxButton = CreateButton("Maximize", titleBar.transform, "□", new Vector2(-78f, -4f));
            Button closeButton = CreateButton("Close", titleBar.transform, "×", new Vector2(-36f, -4f));
            minButton.onClick.AddListener(window.Minimize);
            maxButton.onClick.AddListener(window.ToggleMaximize);
            closeButton.onClick.AddListener(window.Close);

            CreateText(
                "Body",
                windowObject.transform,
                "AeroWindowKit v1.1\n\nDrag the title bar. Drop it near the left/right edge to snap, or the top edge to maximize.\n\nUse the bottom-right grip to resize. Minimize to the taskbar, restore, and click to focus.",
                18,
                TextAnchor.MiddleCenter,
                new Vector2(32f, 64f),
                new Vector2(-32f, -72f));

            GameObject grip = CreateUIObject("Resize Grip", windowObject.transform, typeof(Image), typeof(AeroWindowResizeHandle));
            RectTransform gripRect = grip.GetComponent<RectTransform>();
            gripRect.anchorMin = new Vector2(1f, 0f);
            gripRect.anchorMax = new Vector2(1f, 0f);
            gripRect.pivot = new Vector2(1f, 0f);
            gripRect.sizeDelta = new Vector2(22f, 22f);
            gripRect.anchoredPosition = new Vector2(-4f, 4f);
            grip.GetComponent<Image>().color = new Color(0.08f, 0.48f, 0.78f, 0.55f);

            Selection.activeGameObject = windowObject;
            EditorUtility.SetDirty(canvas.gameObject);
            Debug.Log("AeroWindowKit v1.1 demo created. Press Play to test resize and edge snapping.");
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] extraComponents)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            foreach (System.Type type in extraComponents)
            {
                if (go.GetComponent(type) == null)
                    go.AddComponent(type);
            }
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor alignment, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject textObject = CreateUIObject(name, parent, typeof(Text));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.04f, 0.10f, 0.16f, 1f);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition)
        {
            GameObject buttonObject = CreateUIObject(name, parent, typeof(Image), typeof(Button));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(36f, 32f);
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.92f, 0.98f, 1f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", buttonObject.transform, label, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            text.raycastTarget = false;
            return button;
        }
    }
}
