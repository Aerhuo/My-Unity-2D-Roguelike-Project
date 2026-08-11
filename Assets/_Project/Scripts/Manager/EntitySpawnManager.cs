using System.Collections.Generic;
using UnityEngine;

public class EntitySpawnManager : MonoBehaviour
{
    public static EntitySpawnManager Instance { get; private set; }
    public List<EntityDataSO> dataSOs;
    private MapManager Map => MapManager.Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        List<EntityDataSO> sortedList = new();
        foreach (var data in dataSOs)
        {
            if (data == null) continue;
            while (sortedList.Count <= data.entityID)
            {
                sortedList.Add(null);
            }
            sortedList[data.entityID] = data;
        }
        dataSOs = sortedList;
    }

    public IEntity SpawnAtPosition(int id, Vector2Int pos)
    {
        if (id < 0 || id >= dataSOs.Count || dataSOs[id] == null || pos.x == -1) return null;

        GameObject obj = Instantiate(dataSOs[id].prefab, MapManager.GridToWorld(pos), Quaternion.identity);
        IEntity entity = obj.GetComponent<IEntity>();

        if (entity.Service.TryGet<IEntitySpwaner>(out var controller))
        {
            controller.TriggerSpawn(pos);
        }

        return entity;
    }

    public IEntity[] Spawn(int id, int roomId, int count, int safeDistance = 0)
    {
        if (id < 0 || id >= dataSOs.Count || dataSOs[id] == null || roomId < 0 || roomId >= Map.RoomsCount) return null;

        List<IEntity> spawnedList = new();

        for (int i = 0; i < count; ++i)
        {
            Vector2Int pos = new Vector2Int(-1, -1);
            int maxTry = 50;

            for (int tryCount = 0; tryCount < maxTry; ++tryCount)
            {
                Vector2Int tempPos = Map.GetRandomFloor(roomId);
                if (tempPos.x == -1) continue;

                if (safeDistance > 0 && GameManager.Instance.Player != null)
                {
                    int dist = MapManager.GetDist(tempPos, GameManager.Instance.Player.Pos);
                    if (dist < safeDistance) continue;
                }

                pos = tempPos;
                break;
            }

            if (pos.x == -1) continue;

            IEntity entity = SpawnAtPosition(id, pos);
            if (entity != null) spawnedList.Add(entity);
        }

        return spawnedList.ToArray();
    }

    public IEntity[] Spawn(int id, int count, int safeDistance = 0)
    {
        if (id < 0 || id >= dataSOs.Count || dataSOs[id] == null) return null;

        List<IEntity> spawnedList = new();

        for (int i = 0; i < count; ++i)
        {
            Vector2Int pos = new Vector2Int(-1, -1);
            int maxTry = 50;

            for (int tryCount = 0; tryCount < maxTry; ++tryCount)
            {
                Vector2Int tempPos = Map.GetRandomFloor();
                if (tempPos.x == -1) continue;

                if (safeDistance > 0 && GameManager.Instance.Player != null)
                {
                    int dist = MapManager.GetDist(tempPos, GameManager.Instance.Player.Pos);
                    if (dist < safeDistance) continue;
                }

                pos = tempPos;
                break;
            }

            if (pos.x == -1) continue;

            IEntity entity = SpawnAtPosition(id, pos);
            if (entity != null) spawnedList.Add(entity);
        }

        return spawnedList.ToArray();
    }
}