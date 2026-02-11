# SandBridgePuzzle – Script Reference & Component Checklist

This document breaks down each script with its public API, what it needs assigned in the Inspector, and how it connects to other systems.

---

## Core Systems

### 1. SandGridManager
**Location**: `Assets/Scripts/SandBridgePuzzle/Core/SandGridManager.cs`

**Purpose**: Stores the 10×25 integer grid (-1 = empty, 0-5 = sand color).

**Public API**:
```csharp
public bool CanPlaceTetromino(Vector2Int pos, Vector2Int[] cells)
public void AddSandPixels(Vector2Int pos, Vector2Int[] cells, SandColor color)
public int[,] GetGridReference()
public int[,] CreateEmptyBuffer()
public void SetGridFromBuffer(int[,] buffer)
public int ClearCells(IEnumerable<Vector2Int> cells)
public Vector2Int WorldToGrid(Vector2 worldPos)
```

**Inspector Setup**:
1. Create empty GameObject: `SandGridManagerObject`
2. Add Component → `SandGridManager`
3. Set **Width**: 10, **Height**: 25
4. No references needed for other objects.

**Who Uses It**: TetrominoController, SandSimulator, SandTilemapRenderer, BridgeDetector

---

### 2. SandSimulator
**Location**: `Assets/Scripts/SandBridgePuzzle/Core/SandSimulator.cs`

**Purpose**: Updates the sand grid every tick (cellular automaton: down + diagonal slide).

**Public API**:
```csharp
public bool SimulateStep()  // Run one tick, returns true if any particle moved
```

**Inspector Setup**:
1. Create empty GameObject: `SandSimulatorObject`
2. Add Component → `SandSimulator`
3. Drag `SandGridManagerObject` into **Grid Manager** field
4. Set **Tick Interval**: 0.08 (12.5 ticks/sec)

**Events**: No public events.

**Who Uses It**: BridgeDetector (calls SimulateStep during chain reaction)

---

### 3. BridgeDetector
**Location**: `Assets/Scripts/SandBridgePuzzle/Core/BridgeDetector.cs`

**Purpose**: Scans for left-to-right color bridges, clears them, triggers sand settlement, repeats until no more bridges (chain reaction).

**Public API**:
```csharp
public void ScanAndClearBridgesWithChains()
public List<Vector2Int> FindBridgingCells()

// Events:
public event Action<int> OnBridgeCleared;                          // fired per clear
public event Action<List<Vector2Int>> OnBridgeClearedCells;        // positions
public event Action<int, int> OnBridgeChainCompleted;              // (totalRemoved, comboCount)
```

**Inspector Setup**:
1. Create empty GameObject: `BridgeDetectorObject`
2. Add Component → `BridgeDetector`
3. Drag `SandGridManagerObject` into **Grid Manager**
4. Drag `SandSimulatorObject` into **Sand Simulator**
5. Set **Scan Interval**: 0.18

**Who Listens**: ScoreManager (OnBridgeChainCompleted), BridgeVFX (OnBridgeClearedCells)

---

### 4. TetrominoController
**Location**: `Assets/Scripts/SandBridgePuzzle/Core/TetrominoController.cs`

**Purpose**: Attached to each falling tetromino. Handles movement, rotation, falling, collision checks, and disintegration.

**Public Fields**:
```csharp
public Vector2Int[] cells;                  // shape cells (set by spawner)
public SandColor color;                     // piece color
public float fallSpeedStepsPerSecond = 1.0f;
public float softDropMultiplier = 6f;
```

**Controls**:
- Left Arrow / A: Move left
- Right Arrow / D: Move right
- Up Arrow / W: Rotate
- Down Arrow: Hold for soft drop
- Space: Hard drop (instant)

**Inspector Setup** (Prefab):
1. Create empty GameObject: `TetrominoPrefab`
2. Add Sprite Renderer child (visual representation)
3. Add Component → `TetrominoController`
4. Set **Color**: Red
5. Leave **Cells** empty (spawner assigns this)
6. Save as **Prefab** in Assets/Prefabs/

**On Collision**: Automatically calls `Disintegrate()` which writes cells to grid and destroys itself.

---

### 5. TetrominoSpawner
**Location**: `Assets/Scripts/SandBridgePuzzle/Core/TetrominoSpawner.cs`

**Purpose**: Spawns tetrominoes at regular intervals.

**Public API**:
```csharp
void SpawnRandom()
```

**Inspector Setup**:
1. Create empty GameObject: `TetrominoSpawner`
2. Add Component → `TetrominoSpawner`
3. Drag prefab from `Assets/Prefabs/TetrominoPrefab` into **Tetromino Prefab**
4. Set **Spawn Grid Position**: (5, 23) — center top
5. Set **Spawn Interval**: 2.0 seconds
6. **Shapes**: Leave blank to use default Square, T, L

**Auto-Detects**: Finds `SandGridManager` via `FindObjectOfType` at runtime.

---

## Visual Systems

### 6. SandTilemapRenderer
**Location**: `Assets/Scripts/SandBridgePuzzle/Visual/SandTilemapRenderer.cs`

**Purpose**: Reflects grid state onto a Tilemap every frame (or at refresh interval).

**Inspector Setup**:
1. Create 2D Tilemap in scene: `SandTilemap`
2. Add Component → `SandTilemapRenderer`
3. Drag `SandGridManagerObject` into **Grid Manager**
4. Set **Color Tiles** array size: 6
5. Assign tiles (in order):
   - [0] Red_Tile
   - [1] Blue_Tile
   - [2] Green_Tile
   - [3] Yellow_Tile
   - [4] Purple_Tile
   - [5] Orange_Tile
6. Set **Refresh Interval**: 0.08

**Performance**: Currently clears and rebuilds all tiles every interval. For optimization, consider diff-based updates.

---

### 7. BridgeVFX
**Location**: `Assets/Scripts/SandBridgePuzzle/Visual/BridgeVFX.cs`

**Purpose**: Spawns a particle effect at the center of cleared cells.

**Inspector Setup**:
1. Create empty GameObject: `BridgeVFXObject`
2. Add Component → `BridgeVFX`
3. Drag `BridgeDetectorObject` into **Bridge Detector**
4. Create or assign a ParticleSystem prefab:
   - Create empty GameObject: `ClearEffect`
   - Add Component → Particle System
   - Configure (Main: 0.5s duration, Color Over Lifetime to fade)
   - Save as `Assets/Prefabs/ClearEffect.prefab`
   - Drag into **Clear Effect Prefab** field
5. Delete the scene instance

**Listens To**: `BridgeDetector.OnBridgeClearedCells` event

---

## UI Systems

### 8. ScoreManager
**Location**: `Assets/Scripts/SandBridgePuzzle/UI/ScoreManager.cs`

**Purpose**: Computes score and combo multiplier; broadcasts updates.

**Public API**:
```csharp
public int GetScore()
public event Action<int> OnScoreChanged;       // new total
public event Action<int> OnComboChanged;       // combo count
```

**Formula**:
```
score = (totalPixelsCleared × pointsPerCell) + (comboCount - 1) × chainFlatBonus
```

**Inspector Setup**:
1. Create empty GameObject: `ScoreManager`
2. Add Component → `ScoreManager`
3. Drag `BridgeDetectorObject` into **Bridge Detector**
4. Set **Points Per Cell**: 10
5. Set **Chain Flat Bonus**: 50

**Listens To**: `BridgeDetector.OnBridgeChainCompleted`

---

### 9. ScoreUI
**Location**: `Assets/Scripts/SandBridgePuzzle/UI/ScoreUI.cs`

**Purpose**: Binds `ScoreManager` events to UI Text elements.

**Inspector Setup**:
1. Go to Canvas (or GameCanvas)
2. Add Component → `ScoreUI`
3. Drag `ScoreManager` (GameObject) into **Score Manager**
4. Create or assign UI Texts:
   - **Score Text**: displays "Score: 1234"
   - **Combo Text**: displays "Combo x3" (hidden if combo ≤ 1)
5. Drag the Text components into the fields

**Auto-Updates**: When ScoreManager fires events, this UI auto-updates.

---

## Data Classes

### 10. SandColor (Enum)
**Location**: `Assets/Scripts/SandBridgePuzzle/Data/SandColor.cs`

```csharp
public enum SandColor
{
    Red = 0,
    Blue = 1,
    Green = 2,
    Yellow = 3,
    Purple = 4,
    Orange = 5
}
```

Used throughout for color indexing in the grid and UI tiles array.

---

### 11. TetrominoShape (Struct)
**Location**: `Assets/Scripts/SandBridgePuzzle/Data/TetrominoShape.cs`

```csharp
[System.Serializable]
public struct TetrominoShape
{
    public string name;
    public Vector2Int[] cells;  // local offsets
}
```

Stored in `TetrominoSpawner.shapes[]` for defining piece layouts.

---

## Quick Setup Checklist

### Manager Setup (Do First)
- [ ] Create `SandGridManagerObject` with `SandGridManager` (Width=10, Height=25)
- [ ] Create `SandSimulatorObject` with `SandSimulator` → refs SandGridManager
- [ ] Create `BridgeDetectorObject` with `BridgeDetector` → refs SandGridManager, SandSimulator

### Rendering Setup
- [ ] Create Tilemap `SandTilemap` with `SandTilemapRenderer`
- [ ] Create 6 color tiles (Red, Blue, Green, Yellow, Purple, Orange)
- [ ] Assign tiles array to SandTilemapRenderer [0-5]
- [ ] Set Refresh Interval: 0.08

### Input & Spawn Setup
- [ ] Create `TetrominoPrefab` with `TetrominoController`, Sprite Renderer
- [ ] Create `TetrominoSpawner` → refs TetrominoPrefab prefab
- [ ] Set Spawn Position (5, 23) and Interval 2.0

### Score & UI Setup
- [ ] Create `GameCanvas` with UI
- [ ] Add `ScoreText` and `ComboText` to Canvas
- [ ] Create `ScoreManager` → refs BridgeDetector
- [ ] Add `ScoreUI` to Canvas → refs ScoreManager, ScoreText, ComboText

### VFX Setup
- [ ] Create `ClearEffect` ParticleSystem prefab
- [ ] Create `BridgeVFXObject` with `BridgeVFX` → refs BridgeDetector, ClearEffect

---

## Typical Data Flow

1. **Spawn**: TetrominoSpawner creates a TetrominoController instance
2. **Fall**: TetrominoController updates every frame, moves down step by step
3. **Collision**: When can't move down, TetrominoController calls Disintegrate()
4. **Write**: AddSandPixels() writes cells to SandGridManager's int[,]
5. **Simulate**: SandSimulator.SimulateStep() runs every 0.08s, moves sand down/diagonally
6. **Render**: SandTilemapRenderer reads grid and updates Tilemap every 0.08s
7. **Detect**: BridgeDetector scans every 0.18s for left-right bridges
8. **Clear**: On found bridge, clears cells, simulator settles sand, re-scans (chain)
9. **Score**: ScoreManager tallies points, fires events
10. **UI**: ScoreUI updates Text elements on score/combo events
11. **VFX**: BridgeVFX spawns particles at cell centroids

---

## Debugging Tips

**Pieces not falling?**
- Check TetrominoController in prefab has correct properties
- Verify spawner can find SandGridManager (add debug log)

**Grid visual frozen?**
- Check SandTilemapRenderer finds SandGridManager and has refresh interval set
- Verify tiles array is filled correctly

**Sand doesn't move?**
- Lower SandSimulator.tickInterval to 0.04 for faster visible updates
- Check SandGridManager grid dimensions match tilemap size

**Bridges never clear?**
- Lower BridgeDetector.scanInterval to 0.1 for testing
- Add debug logs in FindBridgingCells() to see what's found

**Score/Combo frozen?**
- Verify ScoreUI references are assigned
- Check ScoreManager finds BridgeDetector
- Confirm events are wired (no nulls)

---

End of Reference. Follow the **Quick Setup Checklist** in order, then **Play** and test!
