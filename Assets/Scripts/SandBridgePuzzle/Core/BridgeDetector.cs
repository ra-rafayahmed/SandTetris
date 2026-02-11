using System;
using System.Collections.Generic;
using UnityEngine;
using SandBridgePuzzle.Data;

namespace SandBridgePuzzle.Core
{
    /// <summary>
    /// Detects left-to-right bridges of same-colored sand using BFS (4-directional).
    /// When a bridging connected component is found it clears only those pixels.
    /// Fires the `OnBridgeCleared` event with the number of cleared pixels.
    /// </summary>
    public class BridgeDetector : MonoBehaviour
    {
        public SandGridManager gridManager;
        public SandSimulator sandSimulator;
        [Tooltip("Seconds between bridge scans")]
        public float scanInterval = 0.18f;

        private float accumulator = 0f;

        public event Action<int> OnBridgeCleared;
        /// <summary>
        /// Fired with positions of cleared cells for VFX/spawn targeting.
        /// </summary>
        public event Action<System.Collections.Generic.List<UnityEngine.Vector2Int>> OnBridgeClearedCells;
        /// <summary>
        /// Fired when a full chain of clears completes. (totalRemoved, comboCount)
        /// </summary>
        public event Action<int,int> OnBridgeChainCompleted;

        void Awake()
        {
            if (gridManager == null)
                gridManager = FindFirstObjectByType<SandGridManager>();
        }

        void Update()
        {
            if (gridManager == null) return;
            accumulator += Time.deltaTime;
            if (accumulator >= scanInterval)
            {
                accumulator = 0f;
                ScanAndClearBridgesWithChains();
            }
        }

        /// <summary>
        /// Full scan that clears bridges and resolves chain reactions by letting sand settle
        /// between clears. Fires OnBridgeCleared per clear and OnBridgeChainCompleted after chain finishes.
        /// </summary>
        public void ScanAndClearBridgesWithChains()
        {
            int totalRemoved = 0;
            int combo = 0;

            while (true)
            {
                var toClear = FindBridgingCells();
                if (toClear.Count == 0) break;

                int removed = gridManager.ClearCells(toClear);
                totalRemoved += removed;
                combo++;
                OnBridgeCleared?.Invoke(removed);
                OnBridgeClearedCells?.Invoke(new System.Collections.Generic.List<UnityEngine.Vector2Int>(toClear));

                // Let sand settle between clears. Run up to max steps or until no movement.
                if (sandSimulator != null)
                {
                    const int maxSettleSteps = 32;
                    for (int i = 0; i < maxSettleSteps; i++)
                    {
                        bool moved = sandSimulator.SimulateStep();
                        if (!moved) break;
                    }
                }
            }

            if (combo > 0)
            {
                OnBridgeChainCompleted?.Invoke(totalRemoved, combo);
            }
        }

        /// <summary>
        /// Returns a list of cell positions that belong to components that touch both left and right walls.
        /// </summary>
        public List<Vector2Int> FindBridgingCells()
        {
            var result = new List<Vector2Int>();
            var grid = gridManager.GetGridReference();
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            bool[,] visited = new bool[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (visited[x, y]) continue;
                int color = grid[x, y];
                if (color == -1) { visited[x, y] = true; continue; }

                var comp = new List<Vector2Int>();
                bool touchesLeft = false;
                bool touchesRight = false;

                var q = new Queue<Vector2Int>();
                q.Enqueue(new Vector2Int(x, y));
                visited[x, y] = true;

                while (q.Count > 0)
                {
                    var p = q.Dequeue();
                    comp.Add(p);
                    if (p.x == 0) touchesLeft = true;
                    if (p.x == width - 1) touchesRight = true;

                    TryEnqueue(p.x + 1, p.y);
                    TryEnqueue(p.x - 1, p.y);
                    TryEnqueue(p.x, p.y + 1);
                    TryEnqueue(p.x, p.y - 1);
                }

                if (touchesLeft && touchesRight)
                    result.AddRange(comp);

                void TryEnqueue(int nx, int ny)
                {
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) return;
                    if (visited[nx, ny]) return;
                    if (grid[nx, ny] != color) return;
                    visited[nx, ny] = true;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }

            return result;
        }
    }
}
