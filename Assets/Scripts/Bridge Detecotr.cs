using UnityEngine;
using System.Collections.Generic;

public class BridgeDetector : MonoBehaviour
{
    public static BridgeDetector Instance;

    void Awake()
    {
        Instance = this;
    }

    public void CheckForBridges()
    {
        var grid = GridManager.Instance.grid;
        int w = GridManager.Instance.width;
        int h = GridManager.Instance.height;

        for (int y = 0; y < h; y++)
        {
            if (grid[0, y].filled)
            {
                Color color = grid[0, y].color;

                if (FloodFill(color, new Vector2Int(0,y)))
                {
                    ClearVisited(color);
                }
            }
        }
    }

    bool FloodFill(Color color, Vector2Int start)
    {
        var grid = GridManager.Instance.grid;
        int w = GridManager.Instance.width;
        int h = GridManager.Instance.height;

        Queue<Vector2Int> q = new Queue<Vector2Int>();
        ResetVisited();

        q.Enqueue(start);

        while (q.Count > 0)
        {
            Vector2Int p = q.Dequeue();
            int x = p.x, y = p.y;

            if (!GridManager.Instance.InBounds(x,y)) continue;
            var cell = grid[x,y];

            if (cell.visited || !cell.filled || cell.color != color)
                continue;

            cell.visited = true;

            if (x == w - 1) return true;

            q.Enqueue(new Vector2Int(x+1,y));
            q.Enqueue(new Vector2Int(x-1,y));
            q.Enqueue(new Vector2Int(x,y+1));
            q.Enqueue(new Vector2Int(x,y-1));
        }

        return false;
    }

    void ResetVisited()
    {
        var g = GridManager.Instance.grid;
        int w = GridManager.Instance.width;
        int h = GridManager.Instance.height;

        for(int x=0; x<w; x++)
            for(int y=0; y<h; y++)
                g[x,y].visited = false;
    }

    void ClearVisited(Color color)
    {
        var g = GridManager.Instance.grid;
        int w = GridManager.Instance.width;
        int h = GridManager.Instance.height;

        for(int x=0; x<w; x++)
            for(int y=0; y<h; y++)
                if (g[x,y].visited && g[x,y].color == color)
                    g[x,y].filled = false;
    }
}
