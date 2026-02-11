using UnityEngine;

namespace SandBridgePuzzle.Core
{
    /// <summary>
    /// Simple cellular-automaton sand simulator.
    /// - Uses double-buffering for stable updates
    /// - Movement rules: try down, then diag-left, then diag-right
    /// - Small random bias to alternate diag preference
    /// This is a single-threaded, deterministic-enough baseline. Later we can
    /// replace with Jobs+Burst or compute shader for larger scales.
    /// </summary>
    public class SandSimulator : MonoBehaviour
    {
        public SandGridManager gridManager;

        [Tooltip("Seconds between simulation ticks.")]
        public float tickInterval = 0.08f; // ~12.5 ticks/sec

        private float tickAccumulator = 0f;

        void Awake()
        {
            if (gridManager == null)
                gridManager = FindFirstObjectByType<SandGridManager>();
        }

        void Update()
        {
            if (gridManager == null) return;

            tickAccumulator += Time.deltaTime;
            while (tickAccumulator >= tickInterval)
            {
                tickAccumulator -= tickInterval;
                SimulateStep();
            }
        }

        /// <summary>
        /// Run a single simulation tick. Returns true if any particle moved.
        /// </summary>
        public bool SimulateStep()
        {
            var source = gridManager.GetGridReference();
            int width = source.GetLength(0);
            int height = source.GetLength(1);

            int[,] dest = gridManager.CreateEmptyBuffer();

            bool anyMoved = false;

            // Iterate bottom -> top so that lower particles are considered first (not strictly
            // required since we double-buffer, but keeps behavior intuitive).
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int val = source[x, y];
                    if (val == -1) continue;

                    // Attempt to move down (y-1)
                    int belowY = y - 1;
                    if (belowY >= 0 && source[x, belowY] == -1 && dest[x, belowY] == -1)
                    {
                        dest[x, belowY] = val;
                        anyMoved = true;
                        continue;
                    }

                    // If down is blocked, try diagonals. Randomize which diagonal to prefer.
                    bool tryLeftFirst = Random.value < 0.5f;
                    bool moved = false;

                    for (int i = 0; i < 2 && !moved; i++)
                    {
                        bool tryLeft = (i == 0) ? tryLeftFirst : !tryLeftFirst;
                        int dx = tryLeft ? -1 : 1;
                        int tx = x + dx;
                        int ty = y - 1;
                        if (tx >= 0 && tx < width && ty >= 0)
                        {
                            if (source[tx, ty] == -1 && dest[tx, ty] == -1)
                            {
                                dest[tx, ty] = val;
                                anyMoved = true;
                                moved = true;
                                break;
                            }
                        }
                    }

                    if (moved) continue;

                    // Otherwise, stay in place (if dest cell free). If already taken, stay where possible.
                    if (dest[x, y] == -1)
                        dest[x, y] = val;
                    else
                    {
                        // Find nearest available adjacent cell at same y (fallback), else drop to source
                        bool placed = false;
                        for (int dx = -1; dx <= 1 && !placed; dx++)
                        {
                            int tx = x + dx;
                            if (tx >= 0 && tx < width && dest[tx, y] == -1)
                            {
                                dest[tx, y] = val;
                                placed = true;
                            }
                        }
                        if (!placed)
                        {
                            // last resort: put back into same coordinate (overwrite)
                            dest[x, y] = val;
                        }
                    }
                }
            }

            gridManager.SetGridFromBuffer(dest);
            return anyMoved;
        }
    }
}
