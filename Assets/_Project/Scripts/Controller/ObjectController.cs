using UnityEngine;

public class ObjectController : EntityController, IEntity, IEntitySpawnAndDie, IUseable
{
    public override EntityType EntityType => EntityType.Object;
    public ObjectType ObjectType { get => _objectType; private set => _objectType = value; }
    [SerializeField] private ObjectType _objectType;
    public void Use()
    {
        switch (ObjectType)
        {
            case ObjectType.Stairs: Game.NextFloor();
            break;
        }
    }
}

public enum ObjectType
{
    Stairs
}