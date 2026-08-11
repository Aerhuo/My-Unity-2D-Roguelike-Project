using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum FogState
{
    Unexplored, Explored, Visible
}
public class FogManager : MonoBehaviour
{
    public static FogManager Instance { get; private set; }
    public Tilemap fogTilemap;
    public TileBase unexploredTile;
    public TileBase exploredTile;
    private MapManager Map => MapManager.Instance;
    private FogState[,] Fogs { get; set; }
    public void Init()
    {
        BoundsInt bounds = new(new(-Map.Width, -Map.Height, 0), new(Map.Width * 3, Map.Height * 3, 1));

        long size = Map.Width * 3 * Map.Height * 3;
        TileBase[] tiles = new TileBase[size];
        Array.Fill(tiles, unexploredTile);

        fogTilemap.SetTilesBlock(bounds, tiles);

        Fogs = new FogState[Map.Width, Map.Height];
    }
    public FogState GetFogStateAt(Vector2Int pos) => GetFogStateAt(pos.x, pos.y);
    public FogState GetFogStateAt(int x, int y)
    {
        if (Map.IsOutOfBounds(x, y)) return FogState.Unexplored;

        return Fogs[x, y];
    }
    public void SetFogStateAt(int x, int y, FogState value)
    {
        if (!Map.IsOutOfBounds(x, y))
        {
            if (Fogs[x, y] == value) return;
            else Fogs[x, y] = value;
        }

        switch (value)
        {
            case FogState.Visible: fogTilemap.SetTile(new(x, y), null);
            break;
            case FogState.Explored: fogTilemap.SetTile(new(x, y), exploredTile);
            break;
            case FogState.Unexplored: fogTilemap.SetTile(new(x, y), unexploredTile);
            break;
        }
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}