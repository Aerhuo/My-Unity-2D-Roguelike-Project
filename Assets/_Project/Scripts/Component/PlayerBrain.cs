using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PathTarget))]

[RequireComponent(typeof(GridTransform))]
public class PlayerBrain : GridBehaviour, IBrain, IEntitySpawnAndDie
{
    private PlayerInputReader input;
    private PathTarget pathTarget;
    private ICharacterController controller;
    private PlayerChoseComponent choseComponent;
    private IAttacker attacker;
    private IEntity choseEntity;
    [SerializeField] private float magicCost = 10f;
    public bool IsThinking { get; private set; }
    protected void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        pathTarget = GetComponent<PathTarget>();
        TryGetComponent(out controller);
        TryGetComponent(out choseComponent);
        TryGetComponent(out attacker);
    }
    public void OnSpawn()
    {
        CameraManager.Instance.RegisterTarget(gameObject);
    }

    public void OnDie()
    {
        CameraManager.Instance.UnregisterTarget(gameObject);
    }

    public void ExcuteLogic()
    {
        StartCoroutine(Action());
    }
    protected void OnMoveSuccess()
    {
        pathTarget.UpdateDist();
    }
    private void BeforeAction()
    {
        Dir = Vector2Int.zero;
        if (choseEntity != null)
        {
            Vector2Int pos = choseEntity.Pos;
            if (FogManager.Instance.GetFogStateAt(pos) != FogState.Visible || (choseEntity.Service.TryGet<IDamageable>(out var damageable) && damageable.Death))
            {
                if (choseEntity.Service.TryGet<OnTagComponent>(out var onTagComponent))
                {
                    onTagComponent.UnTag();
                }

                choseEntity = null;
            }
        }
    }
    private IEnumerator Action()
    {
        IsThinking = true;
        BeforeAction();

        bool hasDecided = false;

        while (!hasDecided)
        {
            if (input.IsChosePreesed())
            {
                yield return ChoseLogic();
            }
            else if (input.TryReadDir(out var read))
            {
                MoveLogic(read);
                hasDecided = true;
            }
            else if (input.IsMagicPressed())
            {
                MagicLogic();
                hasDecided = true;
            }

            yield return null;
        }

        controller?.Action();

        IsThinking = false;
    }
    private void MoveLogic(Vector2Int read)
    {
        if (read == Vector2Int.zero)
        {
            if (MapManager.Instance.TryGetFirstEntityOfType<IUseable>(Pos, out var useable)) useable.Use();
        }

        Dir = read;
    }
    private IEnumerator ChoseLogic()
    {
        if (attacker == null) yield break;
        if (choseComponent == null) yield break;

        yield return choseComponent.Chose(Pos, entity =>
        {
            if (entity.Service.TryGet<IDamageable>(out var damageable))
            {
                if (choseEntity != null) 
                {
                    if (choseEntity.Service.TryGet<OnTagComponent>(out var onTagComponent))
                    {
                        onTagComponent.UnTag();
                    }
                }

                choseEntity = entity;
                if (entity.Service.TryGet<OnTagComponent>(out var onTagComponent1))
                {
                    onTagComponent1.OnTag();
                }
            }
        });
    }
    private void MagicLogic()
    {
        if (choseEntity == null) return;
        if (attacker.Mp < magicCost) return;

        attacker.ConsumeMp(magicCost);

        if (choseEntity.Service.TryGet<IDamageable>(out var damageable))
        {
            TurnManager.Instance.PushEvent(() => damageable.TakeDamage(attacker.GetMagicDamage(), AttackType.Magic));
        }
    }
}