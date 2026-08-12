using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class BarComponentBase : MonoBehaviour, IUIInitialize
{
    [SerializeField] private Image fill;
    [SerializeField] private float fillDuration = .3f;
    [SerializeField] private float leftOffset = .2187f;
    [SerializeField] private float rightOffset = .15f;

    private float FillAmount
    {
        get => Mathf.InverseLerp(leftOffset, 1f - rightOffset, fill.fillAmount);
        set => fill.fillAmount = Mathf.Lerp(leftOffset, 1f - rightOffset, value);
    }

    protected abstract float targetPercent { get; }
    protected abstract void BindPlayer();

    private Coroutine smoothCoroutine;
    private float currentTarget = -1f;

    private IEnumerator SmoothRoutine(float start, float end)
    {
        float timer = 0f;
        while (timer < fillDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fillDuration;
            float x = t * t * (3f - 2f * t);

            FillAmount = Mathf.Lerp(start, end, x);

            yield return null;
        }

        FillAmount = end;
        smoothCoroutine = null;
    }

    private void Update()
    {
        float amount = FillAmount;
        float hpPercent = targetPercent;

        if (amount != hpPercent && (smoothCoroutine == null || currentTarget != hpPercent))
        {
            if (smoothCoroutine != null)
            {
                StopCoroutine(smoothCoroutine);
            }

            currentTarget = hpPercent;
            smoothCoroutine = StartCoroutine(SmoothRoutine(amount, hpPercent));
        }
    }

    public void Init()
    {
        FillAmount = 1f;
        GameManager.Instance.OnPlayerSpawned += BindPlayer;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerSpawned -= BindPlayer;
    }
}