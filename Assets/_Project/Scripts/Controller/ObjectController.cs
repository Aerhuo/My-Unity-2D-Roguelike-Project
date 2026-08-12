using UnityEngine;

public class ObjectController : EntityController, IEntity, IEntitySpawnAndDie, IUseable
{
    public override EntityType EntityType => EntityType.Object;
    public ObjectType ObjectType { get => _objectType; private set => _objectType = value; }
    [SerializeField] private ObjectType _objectType;
    public void Use(IEntity user)
    {
        if (user == null) return;

        switch (ObjectType)
        {
            case ObjectType.Stairs: Game.NextFloor();
            break;
            case ObjectType.HealthPotion: 
            if (user.Service.TryGet<IHealable>(out var healable)) healable.Heal(10f);
            break;
            case ObjectType.ManaPotion:
            if (user.Service.TryGet<IAttacker>(out var attacker)) attacker.RestoreMp(10f);
            break;
        }

        TriggerDie();
    }
}

public enum ObjectType
{
    Stairs, HealthPotion, ManaPotion
}