using UnityEngine;
using SandBridgePuzzle.Data;

namespace SandBridgePuzzle.Core
{
    /// <summary>
    /// Simple tetromino spawner for playtesting.
    /// Provide a `TetrominoController` prefab with default visuals and this will spawn shapes at a spawn column.
    /// If no shapes are configured in inspector, a small default set (Square, T, L) is used.
    /// </summary>
    public class TetrominoSpawner : MonoBehaviour
    {
        public TetrominoController tetrominoPrefab;
        public Vector2Int spawnGridPosition = new Vector2Int(5, 23);
        public float spawnInterval = 1.2f;

        public TetrominoShape[] shapes;

        private float timer = 0f;

        void Start()
        {
            if (shapes == null || shapes.Length == 0)
                InitializeDefaultShapes();
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnRandom();
            }
        }

        void SpawnRandom()
        {
            if (tetrominoPrefab == null)
            {
                Debug.LogError("TetrominoSpawner: assign a TetrominoController prefab.");
                return;
            }

            var idx = Random.Range(0, shapes.Length);
            var shape = shapes[idx];

            var go = Instantiate(tetrominoPrefab, (Vector2)spawnGridPosition, Quaternion.identity);
            var ctrl = go.GetComponent<TetrominoController>();
            if (ctrl != null)
            {
                ctrl.cells = shape.cells;
                ctrl.color = (SandColor)Random.Range(0, 4); // choose from first 4 colors by default
            }
        }

        void InitializeDefaultShapes()
        {
            shapes = new TetrominoShape[3];

            // Square (2x2)
            shapes[0].name = "Square";
            shapes[0].cells = new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) };

            // T
            shapes[1].name = "T";
            shapes[1].cells = new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) };

            // L
            shapes[2].name = "L";
            shapes[2].cells = new Vector2Int[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1) };
        }
    }
}
