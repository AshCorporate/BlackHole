# MainMenu Scene Setup

## Overview
This scene is the entry point of the game. It contains only the mobile menu UI.

## Hierarchy

```
MainMenu (Scene root)
├── EventSystem              ← Unity default EventSystem + StandaloneInputModule
├── Main Camera              ← Camera (Background: black or deep space colour)
└── Canvas                   ← Canvas (Screen Space – Overlay, Scaler: Scale With Screen Size 1080×1920)
    ├── Background           ← Image (full-screen, black/space gradient)
    ├── Title                ← TextMeshProUGUI "BLACK HOLE" (large, centred, white)
    ├── Tagline              ← TextMeshProUGUI "Absorb. Capture. Dominate." (small, grey)
    │
    ├── MainPanel            ← VerticalLayoutGroup
    │   ├── PlayButton       ← Button + TextMeshProUGUI "PLAY"
    │   ├── SettingsButton   ← Button + TextMeshProUGUI "SETTINGS"
    │   └── QuitButton       ← Button + TextMeshProUGUI "QUIT"
    │
    └── SettingsPanel        ← SettingsMenu.cs (initially disabled)
        ├── TitleText        ← TextMeshProUGUI "SETTINGS"
        ├── SoundRow
        │   ├── Label        ← TextMeshProUGUI "Sound"
        │   └── SoundToggle  ← Toggle
        ├── SensitivityRow
        │   ├── Label        ← TextMeshProUGUI "Joystick Sensitivity"
        │   └── SensSlider   ← Slider (0.1 – 2.0)
        ├── NameRow
        │   ├── Label        ← TextMeshProUGUI "Player Name"
        │   └── NameInput    ← TMP_InputField
        └── BackButton       ← Button + TextMeshProUGUI "BACK"
```

## Required Components on GameObjects

| GameObject        | Script / Component                          |
|-------------------|---------------------------------------------|
| Canvas            | `Canvas`, `CanvasScaler`, `GraphicRaycaster` |
| MainPanel         | `MainMenu.cs` (assign button refs)           |
| SettingsPanel     | `SettingsMenu.cs` (assign control refs)      |

## Build Settings
- Add `MainMenu` and `Game` scenes to File > Build Settings.
- `MainMenu` should be index 0.
