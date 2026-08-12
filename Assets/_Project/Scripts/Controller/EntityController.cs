using System.Collections;
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
    protected AnimationComponent animationComponent;
    private readonly List<IEntitySpawnAndDie> components = new(10);
    private bool isTriggeredDie = false;
    public void TriggerDestory()
    {
        if (!isTriggeredDie)
        {
            TriggerDie();
        }
        
        Destroy(gameObject);
    }
    protected virtual void Awake()
    {
        Service = GetComponent<ServiceComponent>();
        TryGetComponent(out damageable);
        TryGetComponent(out gridMovement);
        TryGetComponent(out animationComponent);
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
        if (isTriggeredDie) return;
        
        foreach (var component in components) component.OnDie();
        foreach (var component in components) component.OnDieLate();
        isTriggeredDie = true;

        StartCoroutine(WaitAnimationToDie());
    }
    
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
    protected TurnManager Turn => TurnManager.Instance;
    protected FogManager Fog => FogManager.Instance;
    protected MapManager Map => MapManager.Instance;
    protected GameManager Game => GameManager.Instance;
}