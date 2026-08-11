using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridTransform))]
public class PathFinder : MonoBehaviour
{
    public PathTarget pathTarget;
    public Vector2Int FindPath()
    {
        return RunAtsar(pathTarget.GridTransform.Pos, pathTarget);
    }
    public Vector2Int FindPath(Vector2Int pos)
    {
        if (pathTarget != null && pos == pathTarget.GridTransform.Pos) return RunAtsar(pos, pathTarget);
        else return RunAtsar(pos);
    }
    private Dictionary<Vector2Int, int> gScores = new();
    private Dictionary<Vector2Int, Vector2Int> cameFrom = new();
    private PriorityQueue<(Vector2Int, int), int> priorityQueue = new();
    private MapManager Map => MapManager.Instance;
    private GridTransform gridTransform;
    private readonly Vector2Int[] dirs = (Vector2Int[])Directions.Dirs.Clone();
    [SerializeField] private int extraCost = 9;
    private Vector2Int RunAtsar(Vector2Int ePos, PathTarget pathTarget = null)
    {
        gScores.Clear();
        cameFrom.Clear();
        priorityQueue.Clear();

        Vector2Int sPos = gridTransform.Pos;
        gScores[sPos] = 0;
        priorityQueue.Enqueue((sPos, 0), 0);

        while (priorityQueue.Count > 0)
        {
            var (pos, g) = priorityQueue.Dequeue();
            if (gScores[pos] < g) continue;
            if (pos == ePos) break;
            
            foreach (var dir in dirs.Shuffle())
            {
                var cPos = pos + dir;
                if (Map.IsOutOfBounds(cPos) || Map.IsWall(cPos)) continue;

                int ng = g + 1;
                if (Map.HasEntity(cPos)) ng += extraCost;

                int nh;
                if (pathTarget != null) nh = pathTarget.GetDist(cPos.x, cPos.y);
                else nh = MapManager.GetDist(cPos, ePos);

                int nf = ng + nh;
                if (!gScores.TryGetValue(cPos, out int lastG) || ng < lastG)
                {
                    gScores[cPos] = ng;
                    priorityQueue.Enqueue((cPos, ng), nf);
                    cameFrom[cPos] = pos;
                }
            }
        }

        if (!cameFrom.ContainsKey(ePos)) return Vector2Int.zero;

        Vector2Int res = Vector2Int.zero;
        Vector2Int nPos = ePos;
        while (true)
        {
            if (nPos == sPos) break;
            Vector2Int nextPos = cameFrom[nPos];
            res = nPos - nextPos;
            nPos = nextPos;
        }

        return res;
    }
    private void Awake()
    {
        gridTransform = GetComponent<GridTransform>();
    }
}