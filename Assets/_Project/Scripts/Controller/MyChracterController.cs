using UnityEngine;
[RequireComponent(typeof(EntityStats))]
[RequireComponent(typeof(FOVComponent))]

public class MyChracterController : EntityController, ICharacterController, IEntitySpawnAndDie
{
    private FadeComponent fadeComponent;
    private EntityStats entityStats;
    private AnimationComponent animationComponent;
    private FOVComponent fOVComponent;
    private TagComponent tagComponent;
    private IFaction faction;
    protected override void Awake()
    {
        base.Awake();
        entityStats = GetComponent<EntityStats>();
        TryGetComponent(out animationComponent);
        TryGetComponent(out fadeComponent);
        fOVComponent = GetComponent<FOVComponent>();
        TryGetComponent(out tagComponent);
        TryGetComponent(out faction);
    }
    public override EntityType EntityType => EntityType.Chracter;
    private void TryAttack(Vector2Int toPos)
    {
        if (Map.TryGetGridCellAt(toPos, out var gridCell))
        {
            foreach (var entity in gridCell.Entities)
            {
                if (entity.Service.TryGet<IDamageable>(out var damageable) && entity.Service.TryGet<IFaction>(out var faction))
                {
                    if (this.faction.Faction.IsEnemy(faction.Faction))
                    {
                        float damage = entityStats.GetDamage(AttackType.Phisycal);
                        Turn.PushEvent(() => {
                            if (this.damageable != null && this.damageable.Death) return;
                            damageable.TakeDamage(damage, AttackType.Phisycal);
                            });
                    }
                }
            }
        }
    }
    public void Action()
    {
        Vector2Int toPos = Pos + Dir;
        TryAttack(toPos);

        bool moveSuccess = false;
        if (!Map.CanMove(toPos)) toPos = Pos;
        else moveSuccess = true;

        Map.MoveEntities(Pos, toPos, this);
        
        Vector2Int nPos = Pos;
        Pos = toPos;

        TryMove(nPos, toPos);
        if (moveSuccess) OnMoveSuccess();
        
        TurnManager.Instance.PushEvent(() => fOVComponent.UpdateView());
    }
    private void BeforeView()
    {
        viewEnemy = false;
    }
    private void AfterView()
    {
        if (viewEnemy)
        {
            if (tagComponent != null) tagComponent.Tag();
        }
        else
        {
            if (tagComponent != null) tagComponent.Untag();
        }
    }
    private void TryMove(Vector2Int nPos, Vector2Int toPos)
    {
        if (gridMovement == null) return;
        if (fadeComponent == null || !fadeComponent.IsFaded) Turn.PushEvent(() => { if (damageable.Death) return; animationComponent.Play(gridMovement.MoveTo(nPos, toPos)); });
        else Turn.PushEvent(() => { if (damageable.Death) return; gridMovement.Teleport(toPos); } );
    }
    protected void OnMoveSuccess()
    {
    }
    [SerializeField] private bool viewEnemy;
    private void OnView(int x, int y)
    {
        if (!viewEnemy)
        {
            if (!Map.HasEntity(x, y)) return;
            foreach (var entity in Map.GetEntities(x, y))
            {
                if (entity.Service.TryGet<IDamageable>(out var damageable) && entity.Service.TryGet<IFaction>(out var faction))
                {
                    if (this.faction.Faction.IsEnemy(faction.Faction))
                    {
                        viewEnemy = true;
                    }
                }
            }
        }
    }
    public override void OnSpawn()
    {
        base.OnSpawn();
        if (fOVComponent != null)
        {
            fOVComponent.OnEarlyUpdate += BeforeView;
            fOVComponent.OnView += OnView;
            fOVComponent.OnLateUpdate += AfterView;
        }
    }
    public override void OnDie()
    {
        base.OnDie();
        if (fOVComponent != null)
        {
            fOVComponent.OnEarlyUpdate -= BeforeView;
            fOVComponent.OnView -= OnView;
            fOVComponent.OnLateUpdate -= AfterView;
        }
    }
}