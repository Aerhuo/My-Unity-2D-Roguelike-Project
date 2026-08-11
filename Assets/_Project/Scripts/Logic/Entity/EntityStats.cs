using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class EntityStats : MonoBehaviour, IEntitySpawnAndDie, IFaction
{
    public string Name { get => _name; private set => _name = value; }
    [SerializeField] private string _name;
    public float MaxMp { get => _maxMp; private set => _maxMp = value; }
    [SerializeField] private float _maxMp;
    public float PAtk { get => _pAtk; private set => _pAtk = value; }
    [SerializeField] private float _pAtk;
    public float MAtk { get => _mAtk; private set => _mAtk = value; }
    [SerializeField] private float _mAtk;
    private EntityDataSO dataSO;
    private HealthComponent healthComponent;
    public Faction Faction { get => _faction; set => _faction = value; }
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
        PAtk = dataSO.pAtk;
        MAtk = dataSO.mAtk;
        Faction = dataSO.faction;
    }
    public float GetDamage(AttackType attackType)
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
}
public enum AttackType
{
    Phisycal, Magic
}