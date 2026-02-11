using UnityEngine;
using SandBridgePuzzle.Data;

namespace SandBridgePuzzle.Core
{
    /// <summary>
    /// Controls a falling tetromino. Lightweight, grid-aware controller.
    /// Behavior:
    /// - Move left/right by 1 cell on input
    /// - Rotate 90° clockwise
    /// - Soft drop accelerates the fall
    /// - When it cannot move down by one cell, it disintegrates into the sand grid
    /// Notes:
    /// - This implementation uses discrete steps for placement checks (grid cells per step)
    ///   but uses a per-second step rate (`fallSpeedStepsPerSecond`) for smoothness tuning.
    /// - For more continuous physics later, consider path-based collision checks.
    /// </summary>
    public class TetrominoController : MonoBehaviour
    {
        [Header("Shape")]
        [Tooltip("Local integer cell offsets (e.g. (0,0), (1,0), ...) relative to pivot")]
        public Vector2Int[] cells;
        public SandColor color = SandColor.Red;

        [Header("Movement")]
        [Tooltip("How many grid steps per second the piece falls at (1 step = 1 cell)")]
        public float fallSpeedStepsPerSecond = 1.0f;
        [Tooltip("Multiplier applied while holding soft-drop")]
        public float softDropMultiplier = 6f;

        private SandGridManager gridManager;
        private Vector2 position; // continuous position in world units (1 unit == 1 cell)
        private float fallAccumulator = 0f;

        void Awake()
        {
            gridManager = FindFirstObjectByType<SandGridManager>();
            position = transform.position;
        }

        void Update()
        {
            HandleInput();
            FallUpdate();
            // Keep transform in sync with logical position
            transform.position = position;
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                TryMove(Vector2Int.left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                TryMove(Vector2Int.right);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                TryRotate90CW();
            }
            // Hard drop (space) - instantly move down until collision and disintegrate
            if (Input.GetKeyDown(KeyCode.Space))
            {
                while (TryMove(Vector2Int.down)) { }
                Disintegrate();
            }
        }

        void FallUpdate()
        {
            float multiplier = Input.GetKey(KeyCode.DownArrow) ? softDropMultiplier : 1f;
            fallAccumulator += Time.deltaTime * fallSpeedStepsPerSecond * multiplier;

            // When accumulator reaches >=1, attempt to step down one grid cell
            while (fallAccumulator >= 1f)
            {
                fallAccumulator -= 1f;
                if (!TryMove(Vector2Int.down))
                {
                    // Landed - write pixels into the grid and destroy the piece
                    Disintegrate();
                    return;
                }
            }
        }

        bool TryMove(Vector2Int delta)
        {
            Vector2Int currentGrid = Vector2Int.RoundToInt(position);
            Vector2Int targetGrid = currentGrid + delta;
            if (gridManager != null && gridManager.CanPlaceTetromino(targetGrid, cells))
            {
                position += (Vector2)delta;
                return true;
            }
            return false;
        }

        void TryRotate90CW()
        {
            Vector2Int[] rotated = new Vector2Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                // (x,y) -> ( -y, x ) is 90° CW for integer grid coords
                rotated[i] = new Vector2Int(-cells[i].y, cells[i].x);
            }

            Vector2Int gridPos = Vector2Int.RoundToInt(position);
            if (gridManager != null && gridManager.CanPlaceTetromino(gridPos, rotated))
            {
                cells = rotated;
            }
            else
            {
                // Optionally: simple wall-kick: try shifting left or right by one
                if (gridManager != null)
                {
                    if (gridManager.CanPlaceTetromino(gridPos + Vector2Int.left, rotated))
                    {
                        position += Vector2.left;
                        cells = rotated;
                    }
                    else if (gridManager.CanPlaceTetromino(gridPos + Vector2Int.right, rotated))
                    {
                        position += Vector2.right;
                        cells = rotated;
                    }
                }
            }
        }

        void Disintegrate()
        {
            Vector2Int gridPos = Vector2Int.RoundToInt(position);
            if (gridManager != null)
            {
                gridManager.AddSandPixels(gridPos, cells, color);
            }
            Destroy(gameObject);
        }
    }
}
