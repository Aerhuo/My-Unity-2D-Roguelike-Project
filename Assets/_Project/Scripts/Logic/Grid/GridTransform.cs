using UnityEngine;

public class GridTransform : MonoBehaviour
{
    public Vector2Int Pos
    {
        get => pos;
        set
        {
            LastPos = pos;
            pos = value;
        }
    }
    public Vector2Int LastPos { get; private set; }
    public Vector2Int LastDir { get; private set; }
    public Vector2Int Dir
    {
        get => dir;
        set
        {
            LastDir = dir;
            dir = value;
        }
    }
    [SerializeField] private Vector2Int pos;
    [SerializeField] private Vector2Int dir;
}