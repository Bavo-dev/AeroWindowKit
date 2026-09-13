# Upgrading from AeroWindowKit 1.0.0 to 1.1.0

AeroWindowKit 1.1.0 is designed as a drop-in update.

## New file

Add:

- `Assets/AeroWindowKit/Runtime/AeroWindowResizeHandle.cs`

## Replaced files

Replace these with the 1.1.0 versions:

- `Assets/AeroWindowKit/Runtime/AeroWindow.cs`
- `Assets/AeroWindowKit/Runtime/AeroWindowTitleBar.cs`
- `Assets/AeroWindowKit/Editor/AeroWindowKitMenu.cs`
- `Assets/AeroWindowKit/package.json`
- `Assets/AeroWindowKit/README.md`
- `Assets/AeroWindowKit/Documentation/GettingStarted.md`
- `Assets/AeroWindowKit/CHANGELOG.md`
- root `README.md`
- root `CHANGELOG.md`
- root `ROADMAP.md`

## Optional setup changes

Existing 1.0.0 windows continue to work without resize handles.

To enable resizing, create a small UI object on an edge/corner of a window and add `AeroWindowResizeHandle`. A bottom-right grip can keep the default `Right | Bottom` edges.

To save layouts between sessions, enable `Remember Layout` on `AeroWindow` and optionally assign a stable `Persistence Id`.
