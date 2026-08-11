using UnityEngine;

public static class Directions
{
    public static readonly Vector2Int[] Dirs = new Vector2Int[]{Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down};
    public static readonly Vector2Int[] AllDirs = new Vector2Int[]{new(-1, -1), new(-1, 0), new(-1, 1), new(0, -1), new(0, 1), new(1, -1), new(1, 0), new(0, -1)};
}