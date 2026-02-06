using UnityEngine;

public class SandPhysics : MonoBehaviour
{
    void Update()
    {
        // Run physics 3 times per frame for "faster" falling sand
        for(int i = 0; i < 3; i++) 
        {
            ApplyGravity();
        }
        SandManager.Instance.UpdateTexture();
    }

    void ApplyGravity()
    {
        var grid = GridManager.Instance.grid;
        int w = GridManager.Instance.width;
        int h = GridManager.Instance.height;

        for (int y = 1; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                SandCell cell = grid[x, y];
                if (!cell.filled) continue;

                // Down
                if (!grid[x, y - 1].filled) { Swap(x, y, x, y - 1); continue; }

                // Diagonal Left
                if (x > 0 && !grid[x - 1, y - 1].filled) { Swap(x, y, x - 1, y - 1); continue; }

                // Diagonal Right
                if (x < w - 1 && !grid[x + 1, y - 1].filled) { Swap(x, y, x + 1, y - 1); continue; }
            }
        }
    }

    void Swap(int x1, int y1, int x2, int y2)
    {
        var grid = GridManager.Instance.grid;
        SandCell tmp = grid[x1, y1];
        grid[x1, y1] = grid[x2, y2];
        grid[x2, y2] = tmp;
    }
}
