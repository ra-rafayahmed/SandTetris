using UnityEngine;
using UnityEngine.Tilemaps;

namespace SandBridgePuzzle.Core
{
    /// <summary>
    /// Renders the `SandGridManager` grid into a `Tilemap`.
    /// - Assign a Tilemap in the scene and a TileBase[] where index==SandColor enum value.
    /// - The grid origin (0,0) maps to tilemap cell (0,0).
    /// </summary>
    public class SandTilemapRenderer : MonoBehaviour
    {
        public SandGridManager gridManager;
        public Tilemap tilemap;
        [Tooltip("Tile per SandColor index. Leave entries null to render empty.")]
        public TileBase[] colorTiles;

        [Tooltip("Seconds between visual refreshes (can match simulator tick).")]
        public float refreshInterval = 0.08f;

        private float accumulator = 0f;

        void Awake()
        {
            if (gridManager == null)
                gridManager = FindFirstObjectByType<SandGridManager>();
            if (tilemap == null)
                Debug.LogError("SandTilemapRenderer: assign a Tilemap in inspector.");
        }

        void Update()
        {
            if (gridManager == null || tilemap == null) return;
            accumulator += Time.deltaTime;
            if (accumulator >= refreshInterval)
            {
                accumulator = 0f;
                RefreshAllTiles();
            }
        }

        void RefreshAllTiles()
        {
            var grid = gridManager.GetGridReference();
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Clear whole range quickly
            tilemap.ClearAllTiles();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int v = grid[x, y];
                    if (v == -1) continue;
                    if (v >= 0 && v < colorTiles.Length && colorTiles[v] != null)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), colorTiles[v]);
                    }
                }
            }
        }
    }
}
