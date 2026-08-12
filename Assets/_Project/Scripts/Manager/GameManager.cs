using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private MapManager Map => MapManager.Instance;
    private FogManager Fog => FogManager.Instance;
    private UIManager UI => UIManager.Instance;
    private EntitySpawnManager Spawn => EntitySpawnManager.Instance;
    public event Action OnPlayerSpawned;
    public int CurrentFloor { get => _currentFloor; private set => _currentFloor = value; }
    [SerializeField] private int _currentFloor;
    private bool readyToNextFloor;
    public IEntity Player;

    [SerializeField] private List<FloorStageSO> stageConfigs;

    private bool _isConfigsSorted = false;
    private readonly List<IEntity> entities = new();
    private readonly List<IEntity> allEntities = new();

    public void RegisterEntity(IEntity entity)
    {
        if (entity == null || entities.Contains(entity)) return;
        entities.Add(entity);
        allEntities.Add(entity);
    }

    public void UnregisterEntity(IEntity entity)
    {
        if (entity == null) return;
        entities.Remove(entity);
    }

    public void NextFloor()
    {
        if (readyToNextFloor) return;
        TurnManager.Instance.OnTurnEnd += Init;
        readyToNextFloor = true;
    }

    private FloorStageSO GetCurrentStageConfig()
    {
        if (!_isConfigsSorted)
        {
            stageConfigs.Sort((a, b) => a.minFloor.CompareTo(b.minFloor));
            _isConfigsSorted = true;
        }

        FloorStageSO targetConfig = stageConfigs[0];
        foreach (var config in stageConfigs)
        {
            if (CurrentFloor >= config.minFloor && CurrentFloor <= config.maxFloor)
            {
                targetConfig = config;
                break;
            }
        }
        return targetConfig;
    }
    private List<IEntity> refreshers = new();
    private void Init()
    {
        TurnManager.Instance.OnTurnEnd -= Init;
        readyToNextFloor = false;

        CurrentFloor++;

        foreach (var entity in entities)
        {
            if (entity.SaveToNextFloor) refreshers.Add(entity);
        }

        foreach (var entity in allEntities.ToList())
        {
            if (!entity.SaveToNextFloor)
            {
                entity.TriggerDestory();
                continue;
            }

            if (entity.Service.TryGet<IDamageable>(out var damageable))
            {
                if (damageable.Death) entity.TriggerDestory();
            }
        }
        allEntities.Clear();

        FloorStageSO stageConfig = GetCurrentStageConfig();

        MapRect rect = new()
        { 
            width = stageConfig.GetMapWidth(CurrentFloor), 
            height = stageConfig.GetMapHeight(CurrentFloor)
        };
        Map.Init(rect, stageConfig.mapGenerator);
        Fog.Init();
        UI.Init();

        GenerateFloorEntities(stageConfig);

        if (Player == null) 
        {
            var pArray = Spawn.Spawn(0, 1);
            if (pArray != null && pArray.Length > 0) Player = pArray[0];

            OnPlayerSpawned?.Invoke();
        }

        foreach (var entity in refreshers)
        {
            if (entity.Service.TryGet(out RefresherComponent component)) component.RefreshComponents();
        }

        refreshers.Clear();
    }

    private void GenerateFloorEntities(FloorStageSO stageConfig)
    {
        Dictionary<SpawnPoolConfigSO, int> spawnedCounts = new Dictionary<SpawnPoolConfigSO, int>();
        foreach (var pool in stageConfig.spawnPools)
        {
            spawnedCounts[pool] = 0;
        }

        for (int i = 0; i < Map.RoomsCount; i++)
        {
            int roomArea = Map.GetRoomSize(i) * 4; 
            if (roomArea <= 0) continue;

            foreach (var pool in stageConfig.spawnPools)
            {
                int budget = stageConfig.GetPoolBudget(pool, CurrentFloor, roomArea);
                spawnedCounts[pool] += ExecuteSpawning(budget, pool, stageConfig, i);
            }
        }

        foreach (var pool in stageConfig.spawnPools)
        {
            if (pool.guaranteeAtLeastOne && spawnedCounts[pool] == 0)
            {
                int minCost = stageConfig.GetMinCost(pool);
                if (minCost != int.MaxValue)
                {
                    int randomRoomId = UnityEngine.Random.Range(0, Map.RoomsCount);
                    ExecuteSpawning(minCost, pool, stageConfig, randomRoomId);
                }
            }
        }
    }

    private int ExecuteSpawning(int budget, SpawnPoolConfigSO pool, FloorStageSO stageConfig, int roomId)
    {
        int safetyNet = 1000;
        int spawnedCount = 0;
        
        while (budget > 0 && safetyNet-- > 0)
        {
            int idToSpawn = stageConfig.GetRandomEntity(pool, budget);
            
            if (idToSpawn == -1) break;

            int cost = Spawn.dataSOs[idToSpawn].spawnCost;
            
            IEntity[] spawned = Spawn.Spawn(idToSpawn, roomId, 1, pool.safeDistanceFromPlayer);
            
            if (spawned != null && spawned.Length > 0)
            {
                budget -= cost;
                spawnedCount++;
            }
        }
        
        return spawnedCount;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Init();
    }
}

public struct MapRect
{
    public int width, height;
}