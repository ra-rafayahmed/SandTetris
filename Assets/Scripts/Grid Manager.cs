using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 100;
    public int height = 150;
    public float cellSize = 0.05f;

    public SandCell[,] grid;

    void Awake()
    {
        Instance = this;
        grid = new SandCell[width, height];

        for(int x=0; x<width; x++)
            for(int y=0; y<height; y++)
                grid[x,y] = new SandCell();
    }

    public bool InBounds(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < width && y < height);
    }

        // Add this to Grid Manager.cs
    public Vector3 GetGridOrigin()
    {
        // This assumes your Grid Manager object is the center of the play area
        return transform.position - new Vector3((width * cellSize) / 2f, (height * cellSize) / 2f, 0);
    }
}
