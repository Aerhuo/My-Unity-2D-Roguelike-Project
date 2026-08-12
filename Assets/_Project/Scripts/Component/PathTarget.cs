using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridTransform))]
public class PathTarget : GridBehaviour, IEntitySpawnAndDie, IRefresher
{
    private (int id, int value)[,] dist;
    private MapManager Map => MapManager.Instance;
    public void UpdateDist()
    {
        RunBFS();
    }
    private Queue<(int x, int y)> grids = new();
    private int curId = 0;
    public int GetDist(int x, int y) => dist[x, y].id == curId ? dist[x, y].value : int.MaxValue;
    public int GetDist(Vector2Int pos) => GetDist(pos.x, pos.y);
    private void RunBFS()
    {
        if (curId == int.MinValue) ClearData();

        grids.Clear();

        curId++;

        dist[GridTransform.Pos.x, GridTransform.Pos.y] = (curId, 0);
        grids.Enqueue((GridTransform.Pos.x, GridTransform.Pos.y));

        while (grids.Count > 0)
        {
            var (x, y) = grids.Dequeue();

            foreach (var dir in Directions.Dirs)
            {
                int cx = x + dir.x, cy = y + dir.y;
                if (Map.IsOutOfBounds(cx, cy) || Map.IsWall(cx, cy)) continue;
                if (dist[cx, cy].id == curId) continue;

                grids.Enqueue((cx, cy));
                dist[cx, cy] = (curId, dist[x, y].value + 1);
            }
        }
    }
    private void ClearData()
    {
        curId = 0;
        Array.Clear(dist, 0, dist.Length);
    }
    private void Init()
    {
        dist = new (int, int)[Map.Width, Map.Height];
        ClearData();
    }
    public void OnSpawn()
    {
        Init();
    }
    public void OnDie()
    {
    }
    private void Awake()
    {
        GridTransform = GetComponent<GridTransform>();
    }

    public void Refresh()
    {
        Init();
    }
}