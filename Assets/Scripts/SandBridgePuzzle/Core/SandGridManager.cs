using UnityEngine;
using SandBridgePuzzle.Data;

namespace SandBridgePuzzle.Core
{
    /// <summary>
    /// Manages the sand grid backing store. Lightweight int[,] where -1 == empty.
    /// Provides fast checks for tetromino placement and methods to write disintegrated pixels.
    /// Coordinate convention: (0,0) == bottom-left cell. One Unity unit == one grid cell.
    /// </summary>
    public class SandGridManager : MonoBehaviour
    {
        [Header("Grid")]
        public int width = 10;
        public int height = 25;

        // -1 = empty, otherwise castable to SandColor
        private int[,] sandGrid;

        void Awake()
        {
            sandGrid = new int[width, height];
            ClearGrid();
        }

        public void ClearGrid()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    sandGrid[x, y] = -1;
        }

        /// <summary>
        /// Can the tetromino with local `cells` be placed at integer grid position `pos`?
        /// </summary>
        public bool CanPlaceTetromino(Vector2Int pos, Vector2Int[] cells)
        {
            foreach (var c in cells)
            {
                int x = pos.x + c.x;
                int y = pos.y + c.y;
                if (x < 0 || x >= width || y < 0 || y >= height)
                    return false;
                if (sandGrid[x, y] != -1)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Write tetromino cells into the sand grid as individual pixels of the given color.
        /// Out-of-bounds cells are ignored.
        /// </summary>
        public void AddSandPixels(Vector2Int pos, Vector2Int[] cells, SandColor color)
        {
            int ci = (int)color;
            foreach (var c in cells)
            {
                int x = pos.x + c.x;
                int y = pos.y + c.y;
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    sandGrid[x, y] = ci;
                }
            }
        }

        /// <summary>
        /// Exposes the underlying grid reference for simulation/renderer.
        /// Be careful: returned array is the internal storage.
        /// </summary>
        public int[,] GetGridReference() => sandGrid;

        /// <summary>
        /// Create a fresh empty buffer matching grid size (filled with -1).
        /// Useful for double-buffered simulation steps.
        /// </summary>
        public int[,] CreateEmptyBuffer()
        {
            int[,] buf = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    buf[x, y] = -1;
            return buf;
        }

        /// <summary>
        /// Overwrite the internal grid with contents from `buffer`.
        /// Buffer must be same dimensions as this grid.
        /// </summary>
        public void SetGridFromBuffer(int[,] buffer)
        {
            if (buffer == null) return;
            int bx = buffer.GetLength(0);
            int by = buffer.GetLength(1);
            if (bx != width || by != height)
            {
                Debug.LogError("SetGridFromBuffer: buffer size mismatch");
                return;
            }

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    sandGrid[x, y] = buffer[x, y];
        }

        /// <summary>
        /// Clear specific cells (set to -1). Returns number of cells cleared.
        /// </summary>
        public int ClearCells(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int> cells)
        {
            int removed = 0;
            foreach (var c in cells)
            {
                int x = c.x;
                int y = c.y;
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (sandGrid[x, y] != -1)
                    {
                        sandGrid[x, y] = -1;
                        removed++;
                    }
                }
            }
            return removed;
        }

        /// <summary>
        /// Convenience: world position (Vector2) -> grid position (Vector2Int).
        /// Assumes 1 unit per cell and origin at world (0,0) matching grid (0,0).
        /// </summary>
        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        }
    }
}
