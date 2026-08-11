using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "实体池", menuName = "地牢/实体池")]
public class SpawnPoolConfigSO : ScriptableObject
{
    [Header("配置标识")]
    public string poolName = "New Pool";

    [Header("生成参数")]
    public float baseDensity = 1f;
    public float densityGrowth = 0.01f;
    
    [Header("特殊规则")]
    public bool guaranteeAtLeastOne = false; 
    public int safeDistanceFromPlayer = 0; 

    [Header("实体池")]
    public List<SpawnPoolEntry> entries = new();
}