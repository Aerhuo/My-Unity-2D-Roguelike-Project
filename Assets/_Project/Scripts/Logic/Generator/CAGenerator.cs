using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CA生成器", menuName = "地图生成器/CA生成器")]
public class CAGenerator : Generator
{
    [SerializeField] [Range(0f, 1f)] private float floorPercent = .48f;
    [SerializeField] [Range(0f, 1f)] private float reConnectPercent = .05f;
    [SerializeField] private int smoothCount = 4;
    protected override void Generate()
    {
        NoisyMap(floorPercent);
        SmoothMap(smoothCount);
        ConnectMap(reConnectPercent);
    }
    private void NoisyMap(float floorPercent)
    {
        for (int x = 0; x < map.Width; ++x)
        {
            for (int y = 0; y < map.Height; ++y)
            {
                if (Random.value < floorPercent) map.Tiles[x, y] = TileType.Floor;
                else map.Tiles[x, y] = TileType.Wall;
            }
        }
    }
    private void SmoothMap(int count)
    {
        TileType[,] bufferTiles = new TileType[map.Width, map.Height];
        for (int i = 0; i < count; ++i)
        {
            for (int x = 0; x < map.Width; ++x)
            {
                for (int y = 0; y < map.Height; ++y)
                {
                    int wallCount = GetAroundWallCount(x, y);
                    if (wallCount < 4) bufferTiles[x, y] = TileType.Floor;
                    else if (wallCount > 4) bufferTiles[x, y] = TileType.Wall;
                    else bufferTiles[x, y] = map.Tiles[x, y];
                }
            }

            (map.Tiles, bufferTiles) = (bufferTiles, map.Tiles);
        }
    }
    private int GetAroundWallCount(int x, int y)
    {
        int count = 0;
        foreach (var dir in Directions.AllDirs)
        {
            int cx = x + dir.x, cy = y + dir.y;
            if (map.IsOutOfBounds(cx, cy) || map.IsWall(cx, cy)) count++;
        }

        return count;
    }
    private void ConnectMap(float reConnectPercent)
    {
        bool[,] visit = new bool[map.Width, map.Height];
        Queue<(int x, int y)> islandQueue = new(), connectQueue = new();
        Vector2Int[,] originPos = new Vector2Int[map.Width, map.Height];
        int[,] owner = new int[map.Width, map.Height];

        int curIdx = 0;

        for (int x = 0; x < map.Width; ++x)
        {
            for (int y = 0; y < map.Height; ++y)
            {
                if (visit[x, y] || map.IsWall(x, y)) continue;

                curIdx++;

                visit[x, y] = true;
                islandQueue.Enqueue((x, y));
                connectQueue.Enqueue((x, y));
                originPos[x, y] = new(x, y);
                owner[x, y] = curIdx;

                map.Rooms.Add(new());
                map.Rooms[^1].Add(new(x, y));

                while (islandQueue.Count > 0)
                {
                    var (cx, cy) = islandQueue.Dequeue();

                    foreach (var dir in Directions.Dirs)
                    {
                        int nx = cx + dir.x, ny = cy + dir.y;
                        if (map.IsOutOfBounds(nx, ny) || map.IsWall(nx, ny) || visit[nx, ny]) continue;

                        visit[nx, ny] = true;
                        islandQueue.Enqueue((nx, ny));
                        connectQueue.Enqueue((nx, ny));
                        originPos[nx, ny] = new(nx, ny);
                        owner[nx, ny] = curIdx;
                        
                        map.Rooms[^1].Add(new(nx, ny));
                    }
                }
            }
        }

        HashSet<(int, int)> edges = new();
        DSU dsu = new(curIdx + 1);
        int maxCount = curIdx * (curIdx - 1) / 2;
        while (connectQueue.Count > 0 && edges.Count < maxCount)
        {
            var (x, y) = connectQueue.Dequeue();
            foreach (var dir in Directions.Dirs)
            {
                int cx = x + dir.x, cy = y + dir.y;

                if (map.IsOutOfBounds(cx, cy)) continue;

                int cOwner = owner[cx, cy], fOwner = owner[x, y];
                if (cOwner == 0)
                {
                    owner[cx, cy] = fOwner;
                    originPos[cx, cy] = originPos[x, y];
                    connectQueue.Enqueue((cx, cy));
                }
                else if (cOwner != fOwner)
                {
                    if (cOwner > fOwner) (cOwner, fOwner) = (fOwner, cOwner);
                    if (edges.Contains((cOwner, fOwner))) continue;
                    
                    edges.Add((cOwner, fOwner));
                    if (dsu.Union(cOwner, fOwner))
                    {
                        Vector2Int sPos = originPos[cx, cy];
                        Vector2Int ePos = originPos[x, y];
                        
                        RunAstar(sPos, ePos);
                    }
                    else if (Random.value < reConnectPercent)
                    {
                        RunAstar(originPos[cx, cy], originPos[x, y]);
                    }
                }
            }
        }
    }
}