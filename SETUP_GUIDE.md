# SandBridgePuzzle – Unity Setup Guide

## Overview
This guide walks through setting up the Sand Tetris / Color Bridge Puzzle game in Unity 2022 LTS+.
The core systems are:
- **TetrominoController** – falling shapes
- **SandGridManager** – 2D grid storage (int[,])
- **SandSimulator** – cellular automaton sand physics
- **SandTilemapRenderer** – visualizes grid on Tilemap
- **BridgeDetector** – detects and clears left→right color bridges
- **ScoreManager** – tracks score/combo
- **BridgeVFX** – particle effects on clear

---

## Step 1: Create the Scene Foundation

### 1a. Create a new Scene
- File → New Scene (2D)
- Save as `Assets/Scenes/GameScene.unity`

### 1b. Set up the Camera
- Select `Main Camera` in hierarchy
- **Position**: (4.5, 12, -10)  ← center of 10-wide × 25-tall grid
- **Size**: 13 (orthogonal)
- **Background**: dark grey (optional, e.g. #1a1a1a)

### 1c. Create Grid Managers (as GameObjects)
1. Right-click in Hierarchy → Create Empty → rename to `GridManagers`
2. Inside `GridManagers`, create three empty children:
   - `SandGridManagerObject`
   - `SandSimulatorObject`
   - `BridgeDetectorObject`

### 1d. Add Core Scripts to Grid Managers

#### SandGridManager Setup
1. Select `SandGridManagerObject`
2. Add Component → `SandGridManager` (from `SandBridgePuzzle.Core`)
3. In Inspector, set:
   - **Width**: 10
   - **Height**: 25

#### SandSimulator Setup
1. Select `SandSimulatorObject`
2. Add Component → `SandSimulator`
3. Drag `SandGridManagerObject` into the **Grid Manager** field
4. Set **Tick Interval**: 0.08 (12.5 ticks/sec)

#### BridgeDetector Setup
1. Select `BridgeDetectorObject`
2. Add Component → `BridgeDetector`
3. Drag `SandGridManagerObject` into **Grid Manager** field
4. Drag `SandSimulatorObject` into **Sand Simulator** field
5. Set **Scan Interval**: 0.18 (matches your physics logic)

---

## Step 2: Create Tilemap Visualization

### 2a. Create a Tilemap
1. Right-click in Hierarchy → 2D Object → Tilemap → Rectangular
2. Name it `SandTilemap`
3. Parent it under a new `Grid` (auto-created)

### 2b. Create Tile Assets (simple colored squares)
1. **Create a folder** `Assets/Tiles/` (or similar)
2. Create 6 simple sprite tiles (1×1 colored squares, 32×32 px):
   - `Tile_Red.png` (red)
   - `Tile_Blue.png` (blue)
   - `Tile_Green.png` (green)
   - `Tile_Yellow.png` (yellow)
   - `Tile_Purple.png` (purple)
   - `Tile_Orange.png` (orange)
3. Set each sprite's **Texture Type** to `Sprite (2D and UI)` and **Pixel Per Unit** to 100
4. Create a Tile asset for each:
   - Right-click in Assets → Create → Tiles → Tile
   - Name it `Red_Tile`, `Blue_Tile`, etc.
   - Drag the corresponding sprite into the **Sprite** field
   - (Optional: set **Collider Type** to Box if needed later)

### 2c. Add Renderer Component
1. Select `SandTilemap`
2. Add Component → `SandTilemapRenderer` (from `SandBridgePuzzle.Core`)
3. In Inspector:
   - Drag `SandGridManagerObject` into **Grid Manager**
   - Set **Color Tiles** array size to 6
   - Assign the 6 tiles in order:
     - [0] = Red_Tile
     - [1] = Blue_Tile
     - [2] = Green_Tile
     - [3] = Yellow_Tile
     - [4] = Purple_Tile
     - [5] = Orange_Tile
   - Set **Refresh Interval** to 0.08 (match simulator tick)

---

## Step 3: Create Tetromino Prefab

### 3a. Create a Tetromino Prefab
1. Create a new empty GameObject in the scene: `TetrominoPrefab`
2. Add a child Sprite Renderer (or multiple for multi-cell visuals):
   - Add Component → Sprite Renderer
   - Assign a placeholder sprite (e.g., white square from built-in)
   - Set **Sorting Order** to 10 (above sand)
3. Add Component → `TetrominoController` (from `SandBridgePuzzle.Core`)
4. In Inspector:
   - **Cells**: (empty array initially; spawner will set this)
   - **Color**: Red
   - **Fall Speed Steps Per Second**: 1.0
   - **Soft Drop Multiplier**: 6.0
5. **Drag this GameObject into** `Assets/Prefabs/` folder as a prefab
6. Delete the instance from the scene

### 3b. Create Tetromino Spawner
1. Create an empty GameObject: `TetrominoSpawner`
2. Add Component → `TetrominoSpawner`
3. In Inspector:
   - Drag the `TetrominoPrefab` (from Prefabs folder) into **Tetromino Prefab**
   - Set **Spawn Grid Position** to (5, 23) [center top]
   - Set **Spawn Interval** to 2.0 (spawn a piece every 2 seconds)
   - **Shapes**: Leave empty or customize (default Square, T, L)

---

## Step 4: Create UI Canvas

### 4a. Create Canvas
1. Right-click in Hierarchy → UI → Canvas
2. Name it `GameCanvas`
3. Set **Canvas Scaler** Render Mode to `Scale With Screen Size` (for responsive UI)

### 4b. Add Score Text
1. Right-click on Canvas → UI → Text (Legacy)
2. Rename to `ScoreText`
3. Set **Rect Transform**:
   - **Anchors**: Top-Left
   - **Pos X**: 20, **Pos Y**: -20
   - **Size**: (400, 100)
4. Set **Text Component**:
   - Text: "Score: 0"
   - Font Size: 40
   - Color: White

### 4c. Add Combo Text
1. Right-click on Canvas → UI → Text (Legacy)
2. Rename to `ComboText`
3. Set **Rect Transform**:
   - **Anchors**: Top-Center
   - **Pos X**: 0, **Pos Y**: -20
   - **Size**: (400, 100)
4. Set **Text Component**:
   - Text: "" (empty initially)
   - Font Size: 50
   - Color: Yellow/Gold
   - Alignment: Center

### 4d. Add ScoreUI Component
1. Select Canvas
2. Add Component → `ScoreUI` (from `SandBridgePuzzle.UI`)
3. In Inspector:
   - Drag `ScoreManager` GameObject (create below) into **Score Manager**
   - Drag `ScoreText` into **Score Text**
   - Drag `ComboText` into **Combo Text**

---

## Step 5: Create Score Manager

1. Create an empty GameObject: `ScoreManager`
2. Add Component → `ScoreManager` (from `SandBridgePuzzle.UI`)
3. In Inspector:
   - Drag `BridgeDetectorObject` into **Bridge Detector**
   - **Points Per Cell**: 10
   - **Chain Flat Bonus**: 50

---

## Step 6: Create Particle Effect & VFX Setup

### 6a. Create a Simple Particle Effect Prefab
1. Create a new empty GameObject: `ClearEffect`
2. Add Component → Particle System
3. Configure (basic bright flash):
   - **Main**: Duration 0.5, Start Lifetime 0.3, Start Speed 8
   - **Emission**: Rate Over Time 20
   - **Shape**: Sphere (radius 0.5)
   - **Color Over Lifetime**: Start white/yellow, fade to transparent
   - **Renderer**: Material from built-in `Default-Particle`
4. Drag into `Assets/Prefabs/ClearEffect.prefab`
5. Delete from scene

### 6b. Create VFX Component
1. Create an empty GameObject: `BridgeVFXObject`
2. Add Component → `BridgeVFX` (from `SandBridgePuzzle.Visual`)
3. In Inspector:
   - Drag `BridgeDetectorObject` into **Bridge Detector**
   - Drag `ClearEffect` prefab into **Clear Effect Prefab**

---

## Step 7: Final Wiring & Testing

### 7a. Scene Hierarchy Summary
```
Scene
 ├─ Main Camera
 ├─ GridManagers
 │  ├─ SandGridManagerObject [SandGridManager]
 │  ├─ SandSimulatorObject [SandSimulator] → refs SandGridManager
 │  └─ BridgeDetectorObject [BridgeDetector] → refs SandGridManager, SandSimulator
 ├─ Grid (Tilemap parent)
 │  └─ SandTilemap [SandTilemapRenderer] → refs SandGridManager
 ├─ TetrominoSpawner [TetrominoSpawner] → prefab ref
 ├─ ScoreManager [ScoreManager] → refs BridgeDetector
 ├─ BridgeVFXObject [BridgeVFX] → refs BridgeDetector, ClearEffect prefab
 └─ GameCanvas
    ├─ ScoreText [Text]
    └─ ComboText [Text]
```

### 7b. Quick Validation Checklist
- [ ] `SandGridManager` Width=10, Height=25
- [ ] `SandSimulator` has SandGridManager assigned
- [ ] `BridgeDetector` has SandGridManager and SandSimulator assigned
- [ ] `SandTilemapRenderer` has SandGridManager and all 6 color tiles
- [ ] `TetrominoSpawner` has TetrominoPrefab assigned
- [ ] `ScoreManager` has BridgeDetector assigned
- [ ] `ScoreUI` (on Canvas) has ScoreManager, ScoreText, ComboText assigned
- [ ] `BridgeVFX` has BridgeDetector and ClearEffect prefab assigned

### 7c. Play and Test
1. Press **Play** in Unity
2. You should see:
   - Tetrominoes spawning from the top
   - Falling sand after pieces land
   - Tilemap updating in real-time
   - Particle effects when bridges clear
   - Score incrementing on clears
   - Combo counter showing on chains

### 7d. Controls
- **Left Arrow / A**: Move left
- **Right Arrow / D**: Move right
- **Up Arrow / W**: Rotate
- **Down Arrow**: Soft drop
- **Space**: Hard drop

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Pieces don't fall | Check `TetrominoController` has grid manager reference, or instantiation isn't finding it |
| Tilemap doesn't update | Check `SandTilemapRenderer` grid manager and refresh interval |
| Sand doesn't simulate | Check `SandSimulator` grid manager and tick interval (should be ≥ 0.04 for visible updates) |
| Bridges never clear | Check `BridgeDetector` scan interval; try lowering it temporarily (0.1) to debug |
| Score/Combo don't show | Check Canvas UI has `ScoreUI` with proper references; verify text components exist |

---

## Optional Enhancements

1. **Sound Effects**: Add an `AudioSource` and hook into `OnBridgeCleared` event.
2. **Screen Shake**: Add a camera shake component listening to `OnBridgeChainCompleted`.
3. **Pause Menu**: Wrap `Time.timeScale` toggle in a manager.
4. **Difficulty Progression**: Increase spawn rate or simulator speed as score climbs.
5. **Better Visuals**: Use a sprite atlas for tetromino pieces (4 cells per prefab child).
6. **Performance**: Optimize `SandSimulator` with Jobs+Burst for 10k+ particles.

---

## Files to Verify Are in Workspace

```
Assets/Scripts/SandBridgePuzzle/
 ├─ Core/
 │  ├─ TetrominoController.cs
 │  ├─ SandGridManager.cs
 │  ├─ SandSimulator.cs
 │  ├─ BridgeDetector.cs
 │  └─ TetrominoSpawner.cs
 ├─ Data/
 │  ├─ SandColor.cs
 │  └─ TetrominoShape.cs
 ├─ UI/
 │  ├─ ScoreManager.cs
 │  └─ ScoreUI.cs
 └─ Visual/
+     ├─ SandTilemapRenderer.cs
+     └─ BridgeVFX.cs
```

---

**Happy developing!** If you hit setup issues, verify each component's public fields are non-null after wiring.
