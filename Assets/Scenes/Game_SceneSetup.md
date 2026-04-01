# Game Scene Setup

## Overview
The main gameplay scene. All game systems are wired together here.

## Hierarchy

```
Game (Scene root)
├── EventSystem
├── Main Camera              ← Camera (Background: very dark grey), follows player
│
├── ── SYSTEMS ──────────────────────────────────────────────────────
├── GameManager              ← GameManager.cs  (assign all references in Inspector)
├── Map                      ← MapGenerator.cs
├── TerritorySystem          ← TerritorySystem.cs
├── ObjectSpawner            ← ObjectSpawner.cs (assign CityObject prefab)
├── BuffSpawner              ← BuffSpawner.cs   (assign buff prefabs, or leave empty for auto)
├── ScoreManager             ← ScoreManager.cs
├── MatchTimer               ← MatchTimer.cs    (hook OnTimeUp → GameManager)
│
├── ── UI ───────────────────────────────────────────────────────────
└── Canvas                   ← Canvas (Screen Space – Overlay, 1080×1920)
    │
    ├── HUD                  ← GameHUD.cs
    │   ├── TimerText        ← TextMeshProUGUI (top-centre)
    │   └── LeaderboardPanel
    │       └── LeaderboardText ← TextMeshProUGUI (top-right)
    │
    ├── JoystickArea         ← Joystick.cs (bottom-left, full-screen touch region)
    │   ├── Background       ← Image (semi-transparent circle, assign to Joystick.background)
    │   └── Handle           ← Image (smaller circle, assign to Joystick.handle)
    │
    ├── PauseButton          ← Button, calls GameManager.TogglePause()
    │
    ├── PauseMenu            ← PauseMenu.cs (initially disabled)
    │   ├── Overlay          ← Image (semi-transparent black)
    │   ├── TitleText        ← TextMeshProUGUI "PAUSED"
    │   ├── ResumeButton
    │   ├── SettingsButton
    │   └── MainMenuButton
    │
    ├── GameOverPanel        ← GameOverScreen.cs (initially disabled)
    │   ├── ResultText       ← TextMeshProUGUI
    │   ├── RankingText      ← TextMeshProUGUI
    │   ├── PlayAgainButton
    │   └── MainMenuButton
    │
    └── SettingsMenu         ← SettingsMenu.cs (initially disabled, shared with PauseMenu)
```

## Camera Follow Setup
Attach a simple camera follow script (or use Cinemachine):

```csharp
// CameraFollow.cs — attach to Main Camera
void LateUpdate() {
    if (target == null) return;
    Vector3 pos = target.position;
    pos.z = -10f;
    transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 5f);
}
```

## Prefab Setup

### BlackHole (Player & Bot)
```
BlackHole (Prefab root)
├── SpriteRenderer          ← black hole sprite (circle with glow)
├── Rigidbody2D             ← gravityScale=0, Continuous collision
├── CircleCollider2D        ← isTrigger=true
├── MassSystem.cs
├── BlackHolePhysics.cs
├── TerritoryTrail.cs
├── BlackHoleController.cs
└── [BotAI.cs]              ← only on bot variant
```

### CityObject
```
CityObject (Prefab root)
├── SpriteRenderer          ← placeholder square or circle
├── Rigidbody2D             ← gravityScale=0
└── CityObject.cs
```

## Layer / Tag Setup
- Tag `Player` on the human black hole
- Tag `Bot` on bot black holes  
- Tag `CityObject` on city objects
- Layer `Boundary` for the map border collider

## Physics2D Settings
- Gravity: (0, 0)
- Collision matrix: Player/Bot can collide with CityObject and each other
