using System.Collections.Generic;
using UnityEngine;

namespace SandBridgePuzzle.Visual
{
    /// <summary>
    /// Spawns a particle effect at the centroid of cleared cells.
    /// Assign `clearEffectPrefab` (ParticleSystem) in inspector.
    /// </summary>
    public class BridgeVFX : MonoBehaviour
    {
        public Core.BridgeDetector bridgeDetector;
        public ParticleSystem clearEffectPrefab;

        void Awake()
        {
            if (bridgeDetector == null)
                bridgeDetector = FindFirstObjectByType<Core.BridgeDetector>();
        }

        void OnEnable()
        {
            if (bridgeDetector != null)
                bridgeDetector.OnBridgeClearedCells += HandleClearedCells;
        }

        void OnDisable()
        {
            if (bridgeDetector != null)
                bridgeDetector.OnBridgeClearedCells -= HandleClearedCells;
        }

        void HandleClearedCells(List<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0 || clearEffectPrefab == null) return;

            float sx = 0f, sy = 0f;
            foreach (var c in cells)
            {
                sx += c.x;
                sy += c.y;
            }
            Vector3 avg = new Vector3(sx / cells.Count, sy / cells.Count, 0f);

            var ps = Instantiate(clearEffectPrefab, avg, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax + 0.1f);
        }
    }
}
