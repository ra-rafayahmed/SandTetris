using UnityEngine;

public class SandManager : MonoBehaviour
{
    public static SandManager Instance;

    public Texture2D sandTexture;
    public SpriteRenderer display;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        int w = GridManager.Instance.width;
        int h = GridManager.Instance.height;

        sandTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);

        display.sprite = Sprite.Create(
            sandTexture,
            new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f),
            1f / GridManager.Instance.cellSize
        );
    }

    public void UpdateTexture()
    {
        var grid = GridManager.Instance.grid;

        for (int x = 0; x < sandTexture.width; x++)
        {
            for (int y = 0; y < sandTexture.height; y++)
            {
                if (grid[x, y].filled)
                    sandTexture.SetPixel(x, y, grid[x, y].color);
                else
                    sandTexture.SetPixel(x, y, Color.clear);
            }
        }

        sandTexture.Apply();
    }
}
