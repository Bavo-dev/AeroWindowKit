# Getting started with AeroWindowKit

## Fastest path

Use **Tools > AeroWindowKit > Create Demo UI**, then enter Play Mode.

Try the following:

- Drag the title bar.
- Drop near the left or right edge to snap to half the screen.
- Drop near the top edge to maximize.
- Drag the bottom-right grip to resize.
- Minimize and restore from the taskbar.
- Double-click the title bar to maximize/restore.

## Building your own window

1. Add `AeroWindow` to the window root.
2. Add `AeroWindowTitleBar` to a child title bar.
3. Add one or more `AeroWindowResizeHandle` components to edge/corner UI elements.
4. Set each resize handle's `Edges` field.
5. Add an `AeroWindowManager` under the same Canvas.
6. Optionally add `AeroTaskbar` for minimized-window buttons.

## Snapping

Snapping is enabled by default. `AeroWindow` exposes a `Snap Distance` measured in screen pixels.

You can also snap from code:

```csharp
window.SnapLeft();
window.SnapRight();
window.RestoreSize();
```

## Persistence

Enable `Remember Layout` to store layout data in `PlayerPrefs`.

AeroWindow stores the normal position/size plus whether the window was maximized, snapped left, or snapped right. Set a stable `Persistence Id` if your hierarchy may change between builds.

You can manually control persistence:

```csharp
window.SaveLayout();
window.LoadLayout();
window.ResetSavedLayout();
```

## Notes

- Resize behavior is designed for normal fixed-anchor windows. The built-in demo creates this setup automatically.
- Snapped/maximized layouts use stretch anchors and restore the previous normal layout when moved or resized again.
