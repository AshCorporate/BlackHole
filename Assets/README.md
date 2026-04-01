# Black Hole — Unity 2D Game

> **Black Hole** is a 2D mobile game combining the territory-capture mechanic of **Paper.io** with the physics-based mass-growth mechanic of **Hole.io / Yumy.io**.

---

## 🎮 Gameplay

- Control a **black hole** that grows by absorbing city objects and players
- **Leave a trail** while outside your territory and close it to **capture area** (Paper.io)
- Absorb larger and larger objects as your mass increases
- **6–8 players** (1 human + 5–7 AI bots) compete on a round city map
- **10-minute matches** — win by most territory + mass or by being the last hole standing

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── BlackHoleController.cs   — main player controller
│   │   ├── BlackHolePhysics.cs      — movement + absorption physics
│   │   ├── MassSystem.cs            — mass tracking & scaling
│   │   └── TerritoryTrail.cs        — Paper.io trail & capture
│   ├── AI/
│   │   ├── BotAI.cs                 — AI controller (state machine)
│   │   ├── BotStateMachine.cs       — state base class
│   │   ├── BotDifficulty.cs         — ScriptableObject: bot personality
│   │   └── States/
│   │       ├── PatrolState.cs
│   │       ├── HuntState.cs
│   │       ├── CaptureState.cs
│   │       ├── FleeState.cs
│   │       └── BuffSeekState.cs
│   ├── Map/
│   │   ├── MapGenerator.cs          — procedural round city map
│   │   ├── CityObject.cs            — absorbable city objects
│   │   ├── TerritorySystem.cs       — territory ownership & rendering
│   │   └── ObjectSpawner.cs         — object placement & respawning
│   ├── Buffs/
│   │   ├── BuffBase.cs              — abstract buff base
│   │   ├── SpeedBuff.cs             — 2× speed
│   │   ├── MagnetBuff.cs            — attract nearby objects
│   │   ├── DoubleMassBuff.cs        — 2× mass gain
│   │   ├── ShieldBuff.cs            — trail protection
│   │   ├── GravityPulseBuff.cs      — push enemy holes away
│   │   └── BuffSpawner.cs           — random buff placement
│   ├── UI/
│   │   ├── MainMenu.cs
│   │   ├── GameHUD.cs               — timer, leaderboard
│   │   ├── Joystick.cs              — virtual touch joystick
│   │   ├── PauseMenu.cs
│   │   ├── GameOverScreen.cs
│   │   ├── Leaderboard.cs
│   │   └── SettingsMenu.cs          — sound, sensitivity, name
│   ├── Game/
│   │   ├── GameManager.cs           — match bootstrap & win conditions
│   │   ├── MatchTimer.cs            — 10-minute countdown
│   │   ├── ScoreManager.cs          — mass + territory scoring
│   │   └── GameConfig.cs            — ScriptableObject: all tuneable values
│   └── Utils/
│       ├── ObjectPool.cs
│       ├── MathHelpers.cs
│       └── CameraFollow.cs
├── ScriptableObjects/
│   ├── GameSettings.asset           — create from GameConfig
│   └── BotProfiles/                 — create from BotDifficulty
├── Scenes/
│   ├── MainMenu_SceneSetup.md       — hierarchy guide for MainMenu scene
│   └── Game_SceneSetup.md           — hierarchy guide for Game scene
└── ...
```

---

## 🚀 Getting Started

### Requirements
- **Unity 2022 LTS** or later (2D template)
- **TextMeshPro** package (install via Package Manager)
- **Input System** or legacy Input (legacy used by default)

### Setup Steps

1. Open the project in Unity Editor
2. Go to **File > Build Settings** and add both scenes:
   - `Assets/Scenes/MainMenu.unity` (index 0)
   - `Assets/Scenes/Game.unity` (index 1)
3. Create the **GameConfig** asset:
   - Right-click in Project panel → **Create > BlackHole > GameConfig**
   - Save as `Assets/Resources/GameConfig.asset`
4. Create **BotDifficulty** profiles:
   - Right-click → **Create > BlackHole > BotDifficulty**
   - Create Easy, Normal, Hard variants
5. Follow the scene setup guides in `Assets/Scenes/`
6. Press **Play** ✅

---

## ⚙️ Configuration

All game values are exposed in the **GameConfig** ScriptableObject:

| Parameter              | Default | Description                    |
|------------------------|---------|--------------------------------|
| matchDuration          | 600s    | 10-minute match                |
| botCount               | 6       | Number of bots                 |
| mapRadius              | 50      | World-unit radius of the map   |
| startMass              | 5       | Starting mass per player       |
| baseSpeed              | 6       | Base movement speed            |
| absorptionForce        | 8       | Pull force on nearby objects   |
| speedBoostMultiplier   | 2×      | Speed Boost buff strength      |
| magnetDuration         | 6s      | Magnet buff duration           |
| buffSpawnInterval      | 20s     | Seconds between buff spawns    |

---

## 🤖 Bot AI

Bots use a **State Machine** with 5 states:

| State      | Behaviour                                         |
|------------|---------------------------------------------------|
| Patrol     | Roam map, absorb small objects                    |
| Hunt       | Chase and absorb a smaller black hole             |
| Capture    | Draw and close territory loops                    |
| Flee       | Run from a larger black hole                      |
| BuffSeek   | Move toward a nearby power-up                     |

Bot behaviour is controlled by **BotDifficulty** ScriptableObjects:
- **aggressionFactor** — tendency to hunt
- **territorialFactor** — tendency to capture area
- **cowardFactor** — tendency to flee early
- **speedMultiplier** — movement speed modifier

---

## 📱 Mobile Controls

The **Joystick** appears wherever the player touches the screen (dynamic positioning). Sensitivity is adjustable in Settings.

---

## 🏆 Win Conditions

1. **Timer ends** → player with highest (mass + territory) score wins
2. **Last hole standing** → all other players absorbed
3. **Territory domination** → player captures >80% of the map

---

## 📄 License

MIT — see repository root for details.
