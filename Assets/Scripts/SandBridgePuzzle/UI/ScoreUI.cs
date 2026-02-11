using UnityEngine;
using UnityEngine.UI;

namespace SandBridgePuzzle.UI
{
    /// <summary>
    /// Simple UI binder for score and combo texts. Assign `ScoreManager` in inspector.
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        public ScoreManager scoreManager;
        public Text scoreText;
        public Text comboText;

        void Awake()
        {
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<ScoreManager>();
        }

        void OnEnable()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += UpdateScoreText;
                scoreManager.OnComboChanged += UpdateComboText;
            }
        }

        void OnDisable()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= UpdateScoreText;
                scoreManager.OnComboChanged -= UpdateComboText;
            }
        }

        void UpdateScoreText(int newScore)
        {
            if (scoreText != null) scoreText.text = "Score: " + newScore.ToString();
        }

        void UpdateComboText(int combo)
        {
            if (comboText != null)
            {
                if (combo <= 1) comboText.text = "";
                else comboText.text = "Combo x" + combo.ToString();
            }
        }
    }
}
