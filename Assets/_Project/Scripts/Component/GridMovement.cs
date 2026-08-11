using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GridTransform))]
public class GridMovement : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    public bool IsMoving
    {
        get => _isMoving;
        private set
        {
            // if (animator != null) animator.SetBool(IsMovingHash, value);
            _isMoving = value;
        }
    }
    public void Teleport(Vector2Int toPos)
    {
        Pos = toPos;
        transform.position = MapManager.GridToWorld(toPos);
    }
    public float duration = .25f;
    public float jumpHeight = .5f;
    /// <summary>
    /// 不改变 z 轴位置
    /// </summary>
    /// <param name="toPos"></param>
    /// <returns></returns>
    public IEnumerator MoveTo(Vector2Int nowPos, Vector2Int toPos)
    {
        if (IsMoving) yield break;
        duration = Mathf.Max(.01f, duration);

        IsMoving = true;

        Vector2 sPos = MapManager.GridToWorld(nowPos);
        Vector2 ePos = MapManager.GridToWorld(toPos);

        Pos = toPos;
        if (spriteRenderer != null && gridTransform.Dir.x != 0) spriteRenderer.flipX = gridTransform.Dir.x < 0;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);

            float jumpPos = Mathf.Sin(Mathf.PI * t) * jumpHeight;

            float x = t * t * (3f - 2f * t);
            Vector2 newPos = Vector2.Lerp(sPos, ePos, x);
            transform.position = new(newPos.x, newPos.y + jumpPos, transform.position.z);

            yield return null;
        }

        transform.position = new(ePos.x, ePos.y, transform.position.z);
        IsMoving = false;
    }
    [SerializeField] private bool _isMoving;
    private Vector2Int Pos { get => gridTransform.Pos; set => gridTransform.Pos = value; }
    private GridTransform gridTransform;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        gridTransform = GetComponent<GridTransform>();
        TryGetComponent(out animator);
        TryGetComponent(out spriteRenderer);
    }
}