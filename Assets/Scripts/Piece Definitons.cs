using UnityEngine;

public static class PieceDefinitions
{
    public static Vector2Int[][] AllShapes = new Vector2Int[][]
    {
        // Square
        new Vector2Int[] {
            new Vector2Int(0,0),
            new Vector2Int(1,0),
            new Vector2Int(0,1),
            new Vector2Int(1,1)
        },

        // T
        new Vector2Int[] {
            new Vector2Int(-1,0),
            new Vector2Int(0,0),
            new Vector2Int(1,0),
            new Vector2Int(0,1)
        },

        // L
        new Vector2Int[] {
            new Vector2Int(0,0),
            new Vector2Int(0,1),
            new Vector2Int(0,2),
            new Vector2Int(1,0)
        }
    };

    public static Color[] Colors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green
    };
}
