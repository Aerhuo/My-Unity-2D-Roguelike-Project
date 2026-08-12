using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridMovement))]

[RequireComponent(typeof(PathFinder))]
[RequireComponent(typeof(ServiceComponent))]
public class MagicianEnemyBrain : GridBehaviour, IBrain, IEntitySpawnAndDie
{
    public enum EnemyState
    {
        Wander, Away
    }
    [SerializeField] private EnemyState state;
    private PathFinder pathFinder;
    private IFaction iFaction;
    private FOVComponent fOVComponent;
    private ICharacterController controller;
    private AnimationComponent animationComponent;
    protected void Awake()
    {
        pathFinder = GetComponent<PathFinder>();
        TryGetComponent(out fOVComponent);
        TryGetComponent(out iFaction);
        TryGetComponent(out controller);
        TryGetComponent(out animationComponent);
        TryGetComponent(out attacker);
    }
    private IEntity targetEntity;
    private IAttacker attacker;
    private List<IEntity> entities = new();

    public bool IsThinking { get; private set; }

    private void OnFindEnemy(int x, int y)
    {
        if (iFaction == null) return;

        if (!MapManager.Instance.HasEntity(x, y)) return;
        foreach (var iEntity in MapManager.Instance.GetEntities(x, y))
        {
            if (iEntity.Service.TryGet<IFaction>(out var iFaction) && iEntity.Service.TryGet<PathTarget>(out var pathTarget))
            {
                if (this.iFaction.Faction.IsEnemy(iFaction.Faction))
                {
                    entities.Add(iEntity);
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
    private void Away()
    {
        if (targetEntity == null)
        {
            state = EnemyState.Wander;
            Wander();
            return;
        }

        if (MapManager.GetDist(Pos, targetEntity.Pos) > fOVComponent.ViewRadius / 2)
        {
            MagicAttack();
        }
        else
        {
            Vector2Int chose = Vector2Int.zero;
            foreach (var dir in Directions.Dirs)
            {
                if (!MapManager.Instance.CanMove(Pos + dir)) continue;
                if (pathFinder.pathTarget.GetDist(Pos + dir) > pathFinder.pathTarget.GetDist(Pos + chose)) chose = dir;
            }

            Dir = chose;
            if (Dir == Vector2Int.zero) MagicAttack();
        }
    }
    private void MagicAttack()
    {
        if (attacker == null) return;

        Dir = Vector2Int.zero;
        if (targetEntity.Service.TryGet<IDamageable>(out var damageable))
        {
            TurnManager.Instance.PushEvent(() =>
            {
                animationComponent.Play(animationComponent.MagicAnimation(Pos, targetEntity.Pos));
                damageable.TakeDamage(attacker.GetMagicDamage(), AttackType.Magic);
            });
        }
    }
    public void ExcuteLogic()
    {
        IsThinking = true;

        entities.Clear();
        fOVComponent.UpdateView();
        if (entities.Count > 0)
        {
            if (targetEntity != null && (targetEntity.Service.TryGet<IDamageable>(out var damageable) && damageable.Death || !entities.Contains(targetEntity)))
            {
                targetEntity = null;
                pathFinder.pathTarget = null;
                state = EnemyState.Wander;
            }

            if (targetEntity == null)
            {
                if (entities[0].Service.TryGet<PathTarget>(out var pathTarget))
                {
                    targetEntity = entities[0];
                    pathFinder.pathTarget = pathTarget;
                    state = EnemyState.Away;
                }
            }
        }
        else
        {
            state = EnemyState.Wander;
            pathFinder.pathTarget = null;
            targetEntity = null;
        }

        switch (state)
        {
            case EnemyState.Wander: Wander();
            break;
            case EnemyState.Away: Away();
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