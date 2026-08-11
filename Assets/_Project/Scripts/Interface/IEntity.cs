using System;
using UnityEngine;

public interface IEntitySpawnAndDie
{
    public void OnSpawn() {}
    public void OnDie() {}
    public void OnSpawnLate() {}
    public void OnDieLate() {}
}
public interface IEntitySpwaner
{
    public void TriggerSpawn(Vector2Int spawnPos);
    public void TriggerDie();
}
public enum EntityType
{
    Chracter, Object
}
public interface IEntity
{
    public EntityType EntityType { get; }
    public bool IsBlock { get; }
    public bool SaveToNextFloor { get; }
    public Vector2Int Pos { get; }
    public ServiceComponent Service { get; }
    public void TriggerDestory();
}
public enum Faction
{
    Neutral, Player, Monster
}
public interface IDamageable
{
    public void TakeDamage(float damage, AttackType attackType);
    public void Die();
    public event Action OnDieEvent;
    public event Action OnTakeDamage;
    public bool Death { get; }
}
public interface IFaction
{
    public Faction Faction { get; }
}
public interface IUseable
{
    public void Use();
}

public interface IAttacker
{
    public float GetPhysicalDamage();
    public float GetMagicDamage();
    public float Mp { get; }
    public void ConsumeMp(float value);
}