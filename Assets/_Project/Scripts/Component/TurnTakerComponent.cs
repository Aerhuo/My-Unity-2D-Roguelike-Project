using System;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class TurnTakerComponent : MonoBehaviour, ITurnTaker, IEntitySpawnAndDie
{
    [SerializeField] private float curEnergy = 0f;
    private static readonly float perTurnCostEnergy = 100f;
    private IBrain controller;
    private EntityDataSO dataSO;
    public bool CanAct => curEnergy >= perTurnCostEnergy;
    public bool NeedWait => _needWait;
    [SerializeField] private bool _needWait;
    public bool IsActing => controller != null && controller.IsThinking;
    public bool Top => _top;
    [SerializeField] private bool _top;
    private void Awake()
    {
        dataSO = GetComponent<HealthComponent>().DataSO;
        TryGetComponent(out controller);;
    }

    public void TakeTurn()
    {
        controller?.ExcuteLogic();
    }

    public void RecoveryEnergy()
    {
        curEnergy += dataSO.speed;
    }

    public void ConsumeEnergy()
    {
        curEnergy -= perTurnCostEnergy;
    }

    public void OnSpawn()
    {
        TurnManager.Instance.RegisterTaker(this);
    }

    public void OnDie()
    {
        TurnManager.Instance.UnregisterTaker(this);
    }
}