using UnityEngine;

namespace SandBridgePuzzle.Data
{
    /// <summary>
    /// Lightweight container for tetromino cell layouts.
    /// Cells are expressed in local integer coordinates (x,y), centered on a spawn pivot.
    /// </summary>
    [System.Serializable]
    public struct TetrominoShape
    {
        public string name;
        public Vector2Int[] cells;
    }
}
