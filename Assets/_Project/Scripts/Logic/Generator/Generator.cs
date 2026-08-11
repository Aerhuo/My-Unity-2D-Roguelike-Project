using System.Collections.Generic;
using UnityEngine;

public abstract class Generator : ScriptableObject
{
    [SerializeField] private int extraCost = 4;
    public void StartGenerate(MapManager map)
    {
        this.map = map;

        Generate();
    }
    protected Vector2Int[] dirs = (Vector2Int[])Directions.Dirs.Clone();
    protected MapManager map;
    protected abstract void Generate();
    protected PriorityQueue<(Vector2Int pos, int g), int> priorityQueue = new();
    protected Dictionary<Vector2Int, int> gScores = new();
    protected Dictionary<Vector2Int, Vector2Int> cameFrom = new();
    protected void RunAstar(Vector2Int sPos, Vector2Int ePos)
    {
        priorityQueue.Clear();
        gScores.Clear();
        cameFrom.Clear();

        priorityQueue.Enqueue((sPos, 0), 0);
        gScores[sPos] = 0;
        while (priorityQueue.Count > 0)
        {
            var (pos, g) = priorityQueue.Dequeue();
            if (gScores[pos] < g) continue;
            if (pos == ePos) break;

            foreach (var dir in dirs.Shuffle())
            {
                Vector2Int cPos = pos + dir;
                if (map.IsOutOfBounds(cPos)) continue;

                int cost = 1;
                if (map.IsWall(cPos)) cost += extraCost;

                int ng = g + cost;
                int nh = GetDist(cPos, ePos);
                int nf = ng + nh;
                
                if (!gScores.TryGetValue(cPos, out int lastG) || ng < lastG)
                {
                    priorityQueue.Enqueue((cPos, ng), nf);
                    gScores[cPos] = ng;
                    cameFrom[cPos] = pos;
                }
            }
        }

        if (!cameFrom.ContainsKey(ePos)) return;

        Vector2Int nPos = ePos;
        while (true)
        {
            map.Tiles[nPos.x, nPos.y] = TileType.Floor;
            if (nPos == sPos) break;
            nPos = cameFrom[nPos];
        }
    }
    protected int GetDist(Vector2Int pos1, Vector2Int pos2) => Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
}

public class DSU
{
    private int[] parent;
    private int[] sz;
    public DSU(int capacity)
    {
        parent = new int[capacity];
        sz = new int[capacity];
        for (int i = 0; i < capacity; ++i)
        {
            parent[i] = i;
            sz[i] = 1;
        }
    }
    public int Find(int x) => parent[x] == x ? x : parent[x] = Find(parent[x]);
    public bool Union(int x, int y)
    {
        int rx = Find(x), ry = Find(y);
        if (rx == ry) return false;

        if (sz[rx] < sz[ry]) (rx, ry) = (ry, rx);
        parent[ry] = rx;
        sz[rx] += sz[ry];

        return true;
    }
}