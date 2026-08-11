using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Floor, Wall
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    public int Width { get => _width; set => _width = value; }
    public int Height { get => _height; set => _height = value; }
    public TileType[,] Tiles { get; set; }
    public GridCell[,] Grids { get; private set; }
    
    public List<List<Vector2Int>> Rooms { get; private set; } = new();
    public int RoomsSizeSum { get; private set; }
    public int RoomsCount => Rooms.Count;

    public List<Vector2Int>[] AvailableTilesByRoom { get; private set; }
    public int[,] AvailableTilesIndex { get; private set; }

    public Generator generator;
    public Tilemap floorTilemap, wallTilemap;
    public TileBase floorTile, wallTile;

    public void RegisterEntity(IEntity entity, int x, int y)
    {
        if (IsOutOfBounds(x, y)) return;
        if (Grids[x, y] == null) Grids[x, y] = new();
        
        Grids[x, y].AddEntity(entity);
        UpdateTileAvailability(x, y);
    }

    public void UnregisterEntity(IEntity entity, int x, int y)
    {
        if (IsOutOfBounds(x, y) || Grids[x, y] == null) return;

        Grids[x, y].RemoveEntity(entity);
        UpdateTileAvailability(x, y);
    }

    public void MoveEntities(Vector2Int startPos, Vector2Int toPos, IEntity entity)
    {
        if (Grids[startPos.x, startPos.y] != null)
        {
            Grids[startPos.x, startPos.y].RemoveEntity(entity);
            UpdateTileAvailability(startPos.x, startPos.y);
        }

        if (Grids[toPos.x, toPos.y] != null)
        {
            Grids[toPos.x, toPos.y].AddEntity(entity);
            UpdateTileAvailability(toPos.x, toPos.y);
        }
    }

    private void UpdateTileAvailability(int x, int y)
    {
        if (IsOutOfBounds(x, y) || Grids[x, y] == null || Grids[x, y].RoomId == -1) return;

        bool shouldBeAvailable = !Grids[x, y].IsBlock && !IsWall(x, y);
        int currentIndex = AvailableTilesIndex[x, y];
        bool isAvailable = currentIndex != -1;

        if (shouldBeAvailable && !isAvailable)
        {
            int roomId = Grids[x, y].RoomId;
            AvailableTilesIndex[x, y] = AvailableTilesByRoom[roomId].Count;
            AvailableTilesByRoom[roomId].Add(new Vector2Int(x, y));
        }
        else if (!shouldBeAvailable && isAvailable)
        {
            int roomId = Grids[x, y].RoomId;
            var list = AvailableTilesByRoom[roomId];
            int lastIndex = list.Count - 1;
            Vector2Int lastPos = list[lastIndex];

            list[currentIndex] = lastPos;
            AvailableTilesIndex[lastPos.x, lastPos.y] = currentIndex;

            list.RemoveAt(lastIndex);
            AvailableTilesIndex[x, y] = -1;
        }
    }

    public static Vector2 GridToWorld(Vector2Int gpos) => new(gpos.x + .5f, gpos.y + .5f);
    public static Vector2Int WorldToGrid(Vector2 pos) => new((int)pos.x, (int)pos.y);
    public static int GetDist(Vector2Int pos1, Vector2Int pos2) => Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);

    public bool IsOutOfBounds(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height;
    public bool IsOutOfBounds(Vector2Int pos) => IsOutOfBounds(pos.x, pos.y);
    public bool IsWall(int x, int y) => IsOutOfBounds(x, y) || Tiles[x, y] == TileType.Wall;
    public bool IsWall(Vector2Int pos) => IsWall(pos.x, pos.y);

    public bool TryGetFirstEntityOfType<T>(int x, int y, out T res) where T : class
    {
        if (IsOutOfBounds(x, y) || Grids[x, y] == null)
        {
            res = default;
            return false;
        }
        foreach (var iEntity in Grids[x, y].Entities)
        {
            if (iEntity.Service.TryGet<T>(out var component))
            {
                res = component;
                return true;
            }
        }
        res = default;
        return false;
    }

    public bool TryGetFirstEntityOfType<T>(Vector2Int pos, out T res) where T : class => TryGetFirstEntityOfType(pos.x, pos.y, out res);
    
    public bool TryGetGridCellAt(int x, int y, out GridCell gridCell)
    {
        gridCell = default;
        if (!HasEntity(x, y)) return false;
        gridCell = Grids[x, y];
        return true;
    }

    public bool TryGetGridCellAt(Vector2Int pos, out GridCell gridCell) => TryGetGridCellAt(pos.x, pos.y, out gridCell);
    public List<IEntity> GetEntities(int x, int y)
    {
        if (IsOutOfBounds(x, y)) return null;
        return Grids[x, y].Entities;
    }
    public List<IEntity> GetEntities(Vector2Int pos) => GetEntities(pos.x, pos.y);
    public bool HasEntity(int x, int y) => !IsOutOfBounds(x, y) && Grids[x, y] != null && Grids[x, y].Entities.Count != 0;
    public bool HasEntity(Vector2Int pos) => HasEntity(pos.x, pos.y);
    public bool IsBlock(int x, int y) => HasEntity(x, y) && Grids[x, y].IsBlock;
    public bool IsBlock(Vector2Int pos) => IsBlock(pos.x, pos.y);
    public bool CanMove(int x, int y) => !IsOutOfBounds(x, y) && !IsBlock(x, y) && !IsWall(x, y);
    public bool CanMove(Vector2Int pos) => CanMove(pos.x, pos.y);
    public int GetRoomSize(int roomId) => roomId >= RoomsCount ? -1 : Rooms[roomId].Count;

    public Vector2Int GetRandomFloor(int roomId)
    {
        if (roomId < 0 || roomId >= RoomsCount) return new(-1, -1);
        var list = AvailableTilesByRoom[roomId];
        if (list == null || list.Count == 0) return new(-1, -1);
        return list[Random.Range(0, list.Count)];
    }

    public Vector2Int GetRandomFloor()
    {
        int totalAvailable = 0;
        for (int i = 0; i < RoomsCount; i++)
        {
            totalAvailable += AvailableTilesByRoom[i].Count;
        }

        if (totalAvailable == 0) return new Vector2Int(-1, -1);

        int rand = Random.Range(0, totalAvailable);
        
        for (int i = 0; i < RoomsCount; i++)
        {
            if (rand < AvailableTilesByRoom[i].Count)
            {
                return AvailableTilesByRoom[i][rand];
            }
            rand -= AvailableTilesByRoom[i].Count;
        }

        return new Vector2Int(-1, -1);
    }

    public void Init(MapRect mapRect, Generator generator)
    {
        Rooms.Clear();
        RoomsSizeSum = 0;

        Width = mapRect.width >> 1;
        Height = mapRect.height >> 1;
        this.generator = generator;

        Tiles = new TileType[Width, Height];
        generator.StartGenerate(this);
        RenderTiles();

        _width <<= 1;
        _height <<= 1;

        TileType[,] trueTiles = new TileType[Width, Height];
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                trueTiles[x, y] = Tiles[x >> 1, y >> 1];
            }
        }
        Tiles = trueTiles;

        Grids = new GridCell[Width, Height];
        AvailableTilesIndex = new int[Width, Height];
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                Grids[x, y] = new GridCell();
                AvailableTilesIndex[x, y] = -1;
            }
        }

        AvailableTilesByRoom = new List<Vector2Int>[RoomsCount];
        for (int r = 0; r < RoomsCount; r++)
        {
            AvailableTilesByRoom[r] = new List<Vector2Int>();
            RoomsSizeSum += Rooms[r].Count;

            foreach (var pos in Rooms[r])
            {
                for (int dx = 0; dx < 2; dx++)
                {
                    for (int dy = 0; dy < 2; dy++)
                    {
                        int sx = pos.x * 2 + dx;
                        int sy = pos.y * 2 + dy;
                        if (!IsWall(sx, sy))
                        {
                            Grids[sx, sy].RoomId = r;
                            AvailableTilesIndex[sx, sy] = AvailableTilesByRoom[r].Count;
                            AvailableTilesByRoom[r].Add(new Vector2Int(sx, sy));
                        }
                    }
                }
            }
        }
        RoomsSizeSum *= 4;
    }

    [SerializeField] private int _width;
    [SerializeField] private int _height;

    private void RenderTiles()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        BoundsInt rect = new(new(-2, -2, 0), new(Width * 2 + 4, Height * 2 + 4, 1));
        int size = (Width * 2 + 4) * (Height * 2 + 4);
        TileBase[] floorTiles = new TileBase[size];
        TileBase[] wallTiles = new TileBase[size];

        for (int y = -2; y < Height * 2 + 2; ++y)
        {
            for (int x = -2; x < Width * 2 + 2; ++x)
            {
                int idx = (y + 2) * (Width * 2 + 4) + x + 2;
                int realX = x >> 1, realY = y >> 1;
                
                if (IsOutOfBounds(realX, realY)) wallTiles[idx] = wallTile;
                else switch (Tiles[realX, realY])
                {
                    case TileType.Floor: floorTiles[idx] = floorTile;
                        break;
                    case TileType.Wall: wallTiles[idx] = wallTile;
                        break;
                }
            }
        }

        floorTilemap.SetTilesBlock(rect, floorTiles);
        wallTilemap.SetTilesBlock(rect, wallTiles);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}

public class GridCell
{
    public int RoomId { get; set; } = -1;
    public List<IEntity> Entities { get; private set; } = new();
    public void AddEntity(IEntity entity) => Entities.Add(entity);
    public void RemoveEntity(IEntity entity) => Entities.Remove(entity);
    public bool IsBlock => Entities.Any(t => t.IsBlock);
    public IEnumerable<T> GetEntityOfType<T>() => Entities.OfType<T>();
}