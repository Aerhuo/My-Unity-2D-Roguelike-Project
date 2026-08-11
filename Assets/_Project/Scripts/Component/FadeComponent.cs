using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GridTransform))]
public class FadeComponent : MonoBehaviour
{
    [SerializeField] private float fadeDuration = .2f;
    private Vector2Int Pos => gridTransform.Pos;
    private GridTransform gridTransform;
    private SpriteRenderer spriteRenderer;
    private AnimationComponent animationComponent;
    public bool IsFaded { get; private set; }
    public IEnumerator Fade(float start, float end)
    {
        if (spriteRenderer == null) yield break;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(start, end, t);

            spriteRenderer.color = color;

            yield return null;
        }
    }
    public bool UpdateState()
    {
        bool beSeen = FogManager.Instance.GetFogStateAt(Pos.x, Pos.y) == FogState.Visible;
        if (IsFaded && beSeen)
        {
            IsFaded = false;
            if (animationComponent != null) animationComponent.Play(Fade(0f, 1f), false);
            else spriteRenderer.enabled = true;

            return true;
        }
        else if (!IsFaded && !beSeen)
        {
            IsFaded = true;
            if (animationComponent != null) animationComponent.Play(Fade(1f, 0f), false);
            else spriteRenderer.enabled = false;

            return true;
        }

        return false;
    }
    private void Awake()
    {
        TryGetComponent(out spriteRenderer);
        gridTransform = GetComponent<GridTransform>();
        TryGetComponent(out animationComponent);
    }
    private void Update()
    {
        UpdateState();
    }
}