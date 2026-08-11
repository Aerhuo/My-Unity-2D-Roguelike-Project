using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnPoolEntry
{
    public EntityDataSO entityData;
    public int weight = 50;
}

[CreateAssetMenu(menuName = "地牢/地牢数据")]
public class FloorStageSO : ScriptableObject
{
    [Header("层数区间")]
    public int minFloor;
    public int maxFloor = 9999;

    [Header("地图数据")]
    public int baseMapWidth = 20;
    public int baseMapHeight = 20;
    public int sizeGrowthPerFloor = 2;
    public Generator mapGenerator;

    [Header("动态刷新池")]
    public List<SpawnPoolConfigSO> spawnPools = new();

    public int GetMapWidth(int currentFloor) => baseMapWidth + Mathf.Max(0, currentFloor - minFloor) * sizeGrowthPerFloor;
    public int GetMapHeight(int currentFloor) => baseMapHeight + Mathf.Max(0, currentFloor - minFloor) * sizeGrowthPerFloor;

    public int GetPoolBudget(SpawnPoolConfigSO poolConfig, int currentFloor, int totalArea)
    {
        float currentDensity = poolConfig.baseDensity + Mathf.Max(0, currentFloor - minFloor) * poolConfig.densityGrowth;
        return Mathf.FloorToInt(totalArea * currentDensity);
    }

    public int GetMinCost(SpawnPoolConfigSO poolConfig)
    {
        int minCost = int.MaxValue;
        foreach (var entry in poolConfig.entries)
        {
            if (entry.entityData != null && entry.entityData.spawnCost < minCost)
            {
                minCost = entry.entityData.spawnCost;
            }
        }
        return minCost;
    }

    public int GetRandomEntity(SpawnPoolConfigSO poolConfig, int maxCost)
    {
        int totalWeight = 0;
        List<SpawnPoolEntry> validEntries = new();

        foreach (var entry in poolConfig.entries)
        {
            if (entry.entityData == null) continue;
            
            if (entry.entityData.spawnCost <= maxCost)
            {
                validEntries.Add(entry);
                totalWeight += entry.weight;
            }
        }

        if (validEntries.Count == 0) return -1;

        int rand = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var entry in validEntries)
        {
            currentWeight += entry.weight;
            if (rand < currentWeight) return entry.entityData.entityID;
        }

        return validEntries[^1].entityData.entityID;
    }
}