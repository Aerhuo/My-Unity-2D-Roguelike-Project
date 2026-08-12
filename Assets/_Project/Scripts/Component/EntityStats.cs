using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class EntityStats : MonoBehaviour, IEntitySpawnAndDie, IFaction, IAttacker
{
    public string Name { get => _name; private set => _name = value; }
    [SerializeField] private string _name;
    public float MpPercent => MaxMp != 0 ? Mp / MaxMp : 0f;
    public float MaxMp { get => _maxMp; private set => _maxMp = value; }
    [SerializeField] private float _maxMp;
    public float Mp { get => _mp; set => _mp = value; }
    [SerializeField] private float _mp;
    public float PAtk { get => _pAtk; private set => _pAtk = value; }
    [SerializeField] private float _pAtk;
    public float MAtk { get => _mAtk; private set => _mAtk = value; }
    [SerializeField] private float _mAtk;
    private EntityDataSO dataSO;
    private HealthComponent healthComponent;
    public Faction Faction { get => _faction; set => _faction = value; }

    public int ViewRadius { get => _viewRadius; set => _viewRadius = value; }
    [SerializeField] private int _viewRadius;

    [SerializeField] private Faction _faction;

    public void OnSpawn()
    {
        InitData();
    }
    public void OnDie() { }
    public void InitData()
    {
        if (dataSO == null) return;
        Name = dataSO.entityName;
        MaxMp = dataSO.maxMp;
        Mp = MaxMp;
        PAtk = dataSO.pAtk;
        MAtk = dataSO.mAtk;
        Faction = dataSO.faction;
    }
    private float GetDamage(AttackType attackType)
    {
        float resultDamage = 0;
        switch (attackType)
        {
            case AttackType.Phisycal : resultDamage = PAtk;
            break;
            case AttackType.Magic : resultDamage = MAtk;
            break;
        }

        return resultDamage;
    }
    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
        dataSO = healthComponent.DataSO;
    }

    public float GetPhysicalDamage()
    {
        return GetDamage(AttackType.Phisycal);
    }

    public float GetMagicDamage()
    {
        return GetDamage(AttackType.Magic);
    }

    public void ConsumeMp(float value) => Mp = Mathf.Clamp(Mp - value, 0f, MaxMp);
    public void RestoreMp(float value) => Mp = Mathf.Clamp(Mp + value, 0f, MaxMp);
}
public enum AttackType
{
    Phisycal, Magic
}