using UnityEngine;

[System.Serializable]
public class SandCell
{
    public bool filled;
    public Color color;

    // Used only for pathfinding (bridge detection)
    public bool visited;

    public void Reset()
    {
        filled = false;
        visited = false;
        color = Color.clear;
    }
}

