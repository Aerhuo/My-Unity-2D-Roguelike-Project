using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ServiceComponent))]
public abstract class EntityController : GridBehaviour, IEntity, IEntitySpawnAndDie, IRefresher, IEntitySpwaner
{
    public abstract EntityType EntityType { get; }
    public bool IsBlock => _isBlock;
    [SerializeField] private bool _isBlock;
    public bool SaveToNextFloor => _saveToNextFloor;
    [SerializeField] private bool _saveToNextFloor;
    public ServiceComponent Service { get; private set; }
    protected IDamageable damageable;
    protected GridMovement gridMovement;
    private readonly List<IEntitySpawnAndDie> components = new(10);
    public void Destory()
    {
        if (damageable != null) damageable.Die();
        else
        {
            TriggerDie();
            gameObject.SetActive(false);
        }
    }
    protected virtual void Awake()
    {
        Service = GetComponent<ServiceComponent>();
        TryGetComponent(out damageable);
        TryGetComponent(out gridMovement);
        GetComponents(components);
    }
    private void Init()
    {
        Game.RegisterEntity(this);
        Map.RegisterEntity(this, Pos.x, Pos.y);
    }
    public virtual void OnSpawn()
    {
        Init();
    }
    public virtual void OnDie()
    {
        Map.UnregisterEntity(this, Pos.x, Pos.y);
        Game.UnregisterEntity(this);
    }
    public void Refresh()
    {
        Vector2Int toPos = Map.GetRandomFloor();
        if (gridMovement != null) gridMovement.Teleport(toPos);
        Init();
    }
    public virtual void TriggerSpawn(Vector2Int spawnPos)
    {
        Pos = spawnPos;
        foreach (var component in components) component.OnSpawn();
        foreach (var component in components) component.OnSpawnLate();
    }
    public virtual void TriggerDie()
    {
        foreach (var component in components) component.OnDie();
        foreach (var component in components) component.OnDieLate();
    }
    
    protected TurnManager Turn => TurnManager.Instance;
    protected FogManager Fog => FogManager.Instance;
    protected MapManager Map => MapManager.Instance;
    protected GameManager Game => GameManager.Instance;
}