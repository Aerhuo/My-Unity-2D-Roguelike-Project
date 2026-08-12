using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GridTransform))]
public class HealthComponent : MonoBehaviour, IEntitySpawnAndDie, IDamageable, IHealable
{
    public float HpPercent => MaxHealth != 0 ? Health / MaxHealth : 0f;
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
    }
    private IEntitySpwaner entitySpwaner;
    private void Awake()
    {
        TryGetComponent(out entitySpwaner);
    }

    public void OnSpawn()
    {
        Death = false;
        InitData();
    }
    public void Heal(float value)
    {
        Health = Mathf.Clamp(Health + value, 0f, MaxHealth);
    }
}