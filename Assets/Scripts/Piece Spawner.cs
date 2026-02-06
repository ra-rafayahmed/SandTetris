using UnityEngine;
using System.Collections.Generic;

public class PieceSpawner : MonoBehaviour
{
    public static PieceSpawner Instance;

    public GameObject blockVisualPrefab;  // sprite-only prefab
    private List<GameObject> visuals = new List<GameObject>();

    private Vector2Int[] shape;
    private Color color;
    private Vector2Int position;

    float fallTimer = 0f;
    float fallDelay = 0.3f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnPiece();
    }

    void Update()
    {
        HandleInput();
        FallCycle();
        UpdateVisuals();
    }

    void SpawnPiece()
    {
        shape = PieceDefinitions.AllShapes[Random.Range(0, PieceDefinitions.AllShapes.Length)];
        color = PieceDefinitions.Colors[Random.Range(0, PieceDefinitions.Colors.Length)];

        position = new Vector2Int(GridManager.Instance.width / 2, GridManager.Instance.height - 5);

        // Create visuals
        foreach (var _ in shape)
        {
            var obj = Instantiate(blockVisualPrefab);
            obj.GetComponent<SpriteRenderer>().color = color;
            visuals.Add(obj);
        }
    }

    void Melt()
    {
        var grid = GridManager.Instance.grid;

        for (int i = 0; i < shape.Length; i++)
        {
            int gx = position.x + shape[i].x;
            int gy = position.y + shape[i].y;

            if (GridManager.Instance.InBounds(gx, gy))
            {
                grid[gx, gy].filled = true;
                grid[gx, gy].color = color;
            }
        }

        foreach (var v in visuals) Destroy(v);
        visuals.Clear();

        SpawnPiece();
    }

    void FallCycle()
    {
        fallTimer += Time.deltaTime;
        if (fallTimer >= fallDelay)
        {
            fallTimer = 0f;
            TryMove(Vector2Int.down);
        }
    }

    private float inputTimer;
    private float repeatRate = 0.1f;

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) TryMove(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(Vector2Int.right);

        // Holding keys for continuous movement
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            inputTimer += Time.deltaTime;
            if (inputTimer > repeatRate)
            {
                if (Input.GetKey(KeyCode.LeftArrow)) TryMove(Vector2Int.left);
                else TryMove(Vector2Int.right);
                inputTimer = 0;
            }
        }
        else { inputTimer = 0; }
    }

    bool TryMove(Vector2Int dir)
    {
        foreach (var c in shape)
        {
            int x = position.x + c.x + dir.x;
            int y = position.y + c.y + dir.y;

            if (!GridManager.Instance.InBounds(x, y)) 
            {
                if (dir == Vector2Int.down) Melt();
                return false;
            }

            if (GridManager.Instance.grid[x,y].filled)
            {
                if (dir == Vector2Int.down) Melt();
                return false;
            }
        }

        position += dir;
        return true;
    }

    void Rotate()
    {
        for (int i = 0; i < shape.Length; i++)
        {
            var p = shape[i];
            shape[i] = new Vector2Int(-p.y, p.x); // rotate 90°
        }
    }

    void UpdateVisuals()
    {
        if (GridManager.Instance == null) return;

        float cellSize = GridManager.Instance.cellSize;
        Vector3 origin = GridManager.Instance.GetGridOrigin();

        for (int i = 0; i < shape.Length; i++)
        {
            // Calculate target based on grid origin + grid coordinates
            Vector3 targetPos = origin + new Vector3(
                (position.x + shape[i].x) * cellSize + (cellSize / 2f),
                (position.y + shape[i].y) * cellSize + (cellSize / 2f),
                0
            );

            // First time setup? Snap it. Otherwise, Lerp it.
            if (visuals[i].transform.position == Vector3.zero) 
                visuals[i].transform.position = targetPos;

            visuals[i].transform.position = Vector3.Lerp(visuals[i].transform.position, targetPos, Time.deltaTime * 25f);
            
            // Ensure the block visual matches the sand size
            visuals[i].transform.localScale = new Vector3(cellSize, cellSize, 1);
        }
    }
}
