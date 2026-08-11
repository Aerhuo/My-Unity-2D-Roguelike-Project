using UnityEngine;

[RequireComponent(typeof(GridTransform))]
public abstract class GridBehaviour : MonoBehaviour
{
    private GridTransform _gridTransform;
    public GridTransform GridTransform
    {
        get
        {
            if (_gridTransform == null) TryGetComponent(out _gridTransform);
            return _gridTransform;
        }
        set => _gridTransform = value;
    }
    public Vector2Int Pos { get => GridTransform.Pos; set => GridTransform.Pos = value; }
    public Vector2Int Dir { get => GridTransform.Dir; set => GridTransform.Dir = value; }
}