# AeroWindowKit

A lightweight, open-source **desktop-style window system for Unity uGUI**.

![AeroWindowKit preview](preview.png)

AeroWindowKit gives Unity projects app-like windows with dragging, focus ordering, resize handles, edge snapping, minimize-to-taskbar, maximize/restore, optional layout persistence, and close/hide behavior — without requiring TextMeshPro or third-party assets.

## Features

- Drag windows by a title bar
- Keep windows inside the Canvas
- Click-to-focus / bring to front
- **Resize windows from configurable edges or corners**
- **Snap left or right by dragging to a screen edge**
- **Drag to the top edge to maximize**
- Minimize to a runtime taskbar
- Maximize and restore
- **Optional save/restore of window position, size, and snapped/maximized state**
- Hide or destroy on close
- One-click demo creator from the Unity editor
- No third-party dependencies beyond Unity UI (`com.unity.ugui`)
- MIT licensed

## Quick start

### Option A — install as a Unity package from GitHub

In Unity, open **Window > Package Manager > + > Add package from git URL...** and enter:

```text
https://github.com/Bavo-dev/AeroWindowKit.git?path=Assets/AeroWindowKit
```

To pin the package to this release:

```text
https://github.com/Bavo-dev/AeroWindowKit.git?path=Assets/AeroWindowKit#v1.1.0
```

### Option B — copy into a project

Copy `Assets/AeroWindowKit` into your Unity project's `Assets` folder.

Then use:

**Tools > AeroWindowKit > Create Demo UI**

Press Play and try dragging, snapping, resizing, minimizing, maximizing, restoring, and closing the demo window.

## Manual setup

1. Create a Canvas and EventSystem.
2. Add an `AeroWindowManager` anywhere under the Canvas.
3. Add an `AeroTaskbar` to a bottom UI panel and assign its `Button Container`.
4. Add `AeroWindow` to the root of each window.
5. Add `AeroWindowTitleBar` to the title bar and assign the parent `AeroWindow`.
6. Add `AeroWindowResizeHandle` to one or more edge/corner UI objects. Select the edges each handle controls.
7. Wire minimize / maximize / close buttons to `AeroWindow.Minimize`, `ToggleMaximize`, and `Close`.

### Layout persistence

Enable **Remember Layout** on an `AeroWindow` to store its normal position/size plus snapped or maximized state in `PlayerPrefs`.

For production projects, set a stable **Persistence Id** if hierarchy names may change. You can also call:

```csharp
window.SaveLayout();
window.LoadLayout();
window.ResetSavedLayout();
```

## Runtime API

```csharp
using BavoDev.AeroWindowKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    public AeroWindow settingsWindow;

    public void OpenSettings()
    {
        settingsWindow.Show();
        settingsWindow.Focus();
    }

    public void DockSettingsLeft()
    {
        settingsWindow.SnapLeft();
    }
}
```

## Resize handles

`AeroWindowResizeHandle` supports any combination of:

- `Left`
- `Right`
- `Top`
- `Bottom`

For a classic corner grip, use `Right | Bottom`. The window's `Minimum Size` prevents it from being resized too small.

## Compatibility

The package targets modern Unity projects using uGUI and is written for Unity 2022.3 LTS and Unity 6-era APIs. If you find a version-specific issue, open an issue with your Unity version, render mode, and reproduction steps.

## Project structure

```text
Assets/AeroWindowKit/
├── Runtime/          # Runtime scripts
├── Editor/           # One-click demo creator
├── Documentation/    # Extra usage notes
├── Samples~/         # Sample notes
└── package.json      # UPM package metadata
```

## Contributing

Bug reports and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

MIT — see [LICENSE](LICENSE).
