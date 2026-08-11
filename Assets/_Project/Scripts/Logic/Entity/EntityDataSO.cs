using UnityEngine;

[CreateAssetMenu(fileName = "实体数据", menuName = "实体/实体数据")]
public class EntityDataSO : ScriptableObject
{
    public string entityName = "Default";
    public int entityID = 0;
    public float maxHp = 100f;
    public float maxMp = 10f;
    public float pAtk = 20f;
    public float mAtk = 20f;
    public float pDef = 10f;
    public float mDef = 10f;
    public float speed = 20f;
    public int viewRadius = 5;
    public Faction faction;
    public GameObject prefab;
    public int spawnCost = 1;
}