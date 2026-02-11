using UnityEngine;
using SandBridgePuzzle.Core;

namespace SandBridgePuzzle.UI
{
    /// <summary>
    /// Tracks score and combo feedback. Listens to `BridgeDetector` events.
    /// Connect `ScoreUI` via events or assign in inspector.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public BridgeDetector bridgeDetector;
        public int pointsPerCell = 10;
        public int chainFlatBonus = 50;

        private int totalScore = 0;

        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnComboChanged;

        void Awake()
        {
            if (bridgeDetector == null)
                bridgeDetector = FindFirstObjectByType<BridgeDetector>();
        }

        void OnEnable()
        {
            if (bridgeDetector != null)
            {
                bridgeDetector.OnBridgeChainCompleted += HandleChainCompleted;
            }
        }

        void OnDisable()
        {
            if (bridgeDetector != null)
            {
                bridgeDetector.OnBridgeChainCompleted -= HandleChainCompleted;
            }
        }

        void HandleChainCompleted(int totalRemoved, int comboCount)
        {
            int scoreGain = totalRemoved * pointsPerCell + (comboCount - 1) * chainFlatBonus;
            totalScore += Mathf.Max(0, scoreGain);
            OnScoreChanged?.Invoke(totalScore);
            OnComboChanged?.Invoke(comboCount);
            // Optionally: reset combo UI after short delay (handled by UI)
        }

        public int GetScore() => totalScore;
    }
}
