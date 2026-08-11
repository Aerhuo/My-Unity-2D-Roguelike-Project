using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridMovement))]

[RequireComponent(typeof(PathFinder))]
[RequireComponent(typeof(ServiceComponent))]
public class NormalMonsterBrain : GridBehaviour, IBrain, IEntitySpawnAndDie
{
    public enum EnemyState
    {
        Chase, Wander, Away
    }
    [SerializeField] private EnemyState state;
    private PathFinder pathFinder;
    private ServiceComponent service;
    private IFaction iFaction;
    private FOVComponent fOVComponent;
    private ICharacterController controller;
    protected void Awake()
    {
        pathFinder = GetComponent<PathFinder>();
        service = GetComponent<ServiceComponent>();
        TryGetComponent(out fOVComponent);
        TryGetComponent(out iFaction);
        TryGetComponent(out controller);
    }
    private List<PathTarget> enemies = new();

    public bool IsThinking { get; private set; }

    private void OnFindEnemy(int x, int y)
    {
        if (enemies.Count > 0) return;
        if (iFaction == null) return;

        if (!MapManager.Instance.HasEntity(x, y)) return;
        foreach (var iEntity in MapManager.Instance.GetEntities(x, y))
        {
            if (iEntity.Service.TryGet<IFaction>(out var iFaction) && iEntity.Service.TryGet<PathTarget>(out var pathTarget))
            {
                if (this.iFaction.Faction.IsEnemy(iFaction.Faction))
                {
                    enemies.Add(pathTarget);
                }
            }
        }
    }
    private void Wander()
    {
        Dir = GetRandomDir();
    }
    private Vector2Int[] dirs = (Vector2Int[])Directions.Dirs.Clone();
    private Vector2Int GetRandomDir()
    {
        if (dirs == null || dirs.Length == 0) return new(0, 0);

        float weightSum = 0f;
        foreach (var dir in dirs) weightSum += GetDirCost(dir);;

        float needWeight = Random.Range(0f, weightSum);
        float curWeight = 0f;
        foreach (var dir in dirs)
        {
            float weight = GetDirCost(dir);
            if (weight <= 0f) continue;

            curWeight += weight;
            if (curWeight >= needWeight) return dir;
        }

        return dirs[^1];
    }
    private float GetDirCost(Vector2Int dir)
    {
        if (!MapManager.Instance.CanMove(Pos + dir)) return 0f;

        if (dir == Dir) return 40f;
        else if (dir == -Dir) return 10f;
        else return 25f;
    }
    private void Chase()
    {
        Dir = pathFinder.FindPath();
    }
    public void ExcuteLogic()
    {
        IsThinking = true;

        if (enemies.Count > 0)
        {
            if (pathFinder.pathTarget == null)
            {
                pathFinder.pathTarget = enemies[0];
                state = EnemyState.Chase;
            }
        }
        else
        {
            state = EnemyState.Wander;
            pathFinder.pathTarget = null;
        }

        enemies.Clear();

        switch (state)
        {
            case EnemyState.Wander: Wander();
            break;
            case EnemyState.Chase: Chase();
            break;
        }

        if (controller != null) controller.Action();

        IsThinking = false;
    }
    public void OnSpawn()
    {
        if (fOVComponent != null) fOVComponent.OnView += OnFindEnemy;
    }
    public void OnDie()
    {
        if (fOVComponent != null) fOVComponent.OnView -= OnFindEnemy;
    }
}