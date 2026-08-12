using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationType
{
    Attack, TakeDamage, Die
}

public class AnimationComponent : MonoBehaviour, IEntitySpawnAndDie, ITurnWaiter, IRefresher
{
    private static readonly int AttackHash = Animator.StringToHash("attack"); 
    [SerializeField] private GridMovement magicBallMovement;
    private IDamageable damageable;
    private SpriteRenderer spriteRenderer;

    private Color originalColor = Color.white;
    private Vector3 originalScale = Vector3.one;

    public bool Wait => IsPlaying;
    public bool IsPlaying { get; private set; }
    private Queue<IEnumerator> enumerators = new();

    public void Play(AnimationType animation)
    {
        enumerators.Enqueue(PlayAnimation(animation));
    }

    public void Play(IEnumerator enumerator, bool wait = true)
    {
        if (wait)
        {
            enumerators.Enqueue(enumerator);
        }
        else StartCoroutine(enumerator);
    }
    private IEnumerator StartPlay()
    {
        while (true)
        {
            while (enumerators.Count > 0)
            {
                IsPlaying = true;
                yield return enumerators.Dequeue();
            }
            
            IsPlaying = false;
            yield return null;
        }
    }

    private IEnumerator PlayAnimation(AnimationType animation)
    {
        switch (animation)
        {
            case AnimationType.TakeDamage:
                yield return TakeDamageRoutine();
                break;

            case AnimationType.Die:
                yield return DieRoutine();
                break;
        }
    }

    public IEnumerator MagicAnimation(Vector2Int startPos, Vector2Int toPos)
    {
        if (magicBallMovement == null) yield break;
        magicBallMovement.gameObject.SetActive(true);
        yield return magicBallMovement.MoveTo(startPos, toPos);
        magicBallMovement.gameObject.SetActive(false);
    }

    private IEnumerator TakeDamageRoutine()
    {
        if (spriteRenderer == null) yield break;

        Color startColor = spriteRenderer.color;

        for (int i = 1; i <= 5; i++)
        {
            spriteRenderer.color = Color.Lerp(startColor, Color.red, i / 5f);
            yield return null; 
        }

        for (int i = 0; i < 5; i++)
        {
            yield return null;
        }

        for (int i = 1; i <= 5; i++)
        {
            spriteRenderer.color = Color.Lerp(Color.red, startColor, i / 5f);
            yield return null;
        }

        spriteRenderer.color = startColor;
    }

    private IEnumerator DieRoutine()
    {
        if (spriteRenderer == null) yield break;

        Vector3 startScale = transform.localScale;
        Color startColor = spriteRenderer.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        for (int i = 1; i <= 20; i++)
        {
            float t = i / 20f;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            spriteRenderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        spriteRenderer.color = targetColor;
    }

    public void OnDie()
    {
        TurnManager.Instance.UnregisterWaiter(this);
        if (damageable != null)
        {
            damageable.OnTakeDamage -= HandleTakeDamage;
            damageable.OnDieEvent -= HandleDie;
        }
    }
    private void Init()
    {
        StopAllCoroutines();
        enumerators.Clear();
        StartCoroutine(StartPlay());

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        transform.localScale = originalScale;
    }
    public void OnSpawn()
    {
        Init();
        TurnManager.Instance.RegisterWaiter(this);

        if (damageable != null)
        {
            damageable.OnTakeDamage += HandleTakeDamage;
            damageable.OnDieEvent += HandleDie;
        }
    }

    public void Refresh()
    {
        Init();
    }

    private void HandleTakeDamage() => Play(AnimationType.TakeDamage);
    private void HandleDie() => Play(AnimationType.Die);

    private void Awake()
    {
        TryGetComponent(out damageable);
        
        if (TryGetComponent(out spriteRenderer))
        {
            originalColor = spriteRenderer.color;
        }
        originalScale = transform.localScale;
    }
}