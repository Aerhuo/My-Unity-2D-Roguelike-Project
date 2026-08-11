using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GridTransform))]
public class HealthComponent : MonoBehaviour, IEntitySpawnAndDie, IDamageable
{
    public float MaxHealth { get => _maxHp; private set => _maxHp = value; }
    [SerializeField] private float _maxHp;
    public float Health { get => _hp; private set => _hp = value; }
    [SerializeField] private float _hp;
    public event Action OnTakeDamage;
    public event Action OnDieEvent;
    public EntityDataSO DataSO => _dataSO;
    public bool Death { get; private set; }
    [SerializeField] private EntityDataSO _dataSO;
    public float PDef { get => _pDef; private set => _pDef = value; }
    [SerializeField] private float _pDef;
    public float MDef { get => _mDef; private set => _mDef = value; }
    [SerializeField] private float _mDef;
    public void TakeDamage(float damage, AttackType attackType)
    {
        float resultDamage = damage;

        switch (attackType)
        {
            case AttackType.Phisycal: resultDamage -= PDef;
            break;
            case AttackType.Magic: resultDamage -= MDef;
            break;
        }

        resultDamage = Mathf.Max(0f, resultDamage);
        Health -= resultDamage;
        OnTakeDamage?.Invoke();

        if (Health <= 0) Die();
    }
    public void InitData()
    {
        MaxHealth = DataSO.maxHp;
        Health = MaxHealth;
        PDef = _dataSO.pDef;
        MDef = _dataSO.mDef;
    }
    public void Die()
    {
        Death = true;
        OnDieEvent?.Invoke();
        entitySpwaner?.TriggerDie();

        StartCoroutine(WaitAnimationToDie());
    }
    private IEntitySpwaner entitySpwaner;
    private void Awake()
    {
        TryGetComponent(out animationComponent);
        TryGetComponent(out entitySpwaner);
    }

    public void OnSpawn()
    {
        Death = false;
        InitData();
    }
    private AnimationComponent animationComponent;
    private IEnumerator WaitAnimationToDie()
    {
        yield return null;

        if (animationComponent == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        while (animationComponent.Wait) yield return null;

        gameObject.SetActive(false);
    }
    public void OnDie()
    {
    }
}