using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    private readonly List<ITurnTaker> turnTakers = new();
    private readonly List<ITurnWaiter> turnWaiters = new();
    private readonly Queue<Action> turnEventQueue = new();
    public event Action OnTurnEnd;
    public void RegisterTaker(ITurnTaker turnTaker)
    {
        if (turnTaker == null) return;
        turnTakers.Add(turnTaker);
        if (turnTaker.Top) (turnTakers[0], turnTakers[^1]) = (turnTakers[^1], turnTakers[0]);
    }
    public void UnregisterTaker(ITurnTaker turnTaker)
    {
        if (turnTaker != null)
        {
            for (int i = 0; i < turnTakers.Count; ++i) if (turnTakers[i] == turnTaker)
            {
                turnTakers[i] = null;
                break;
            }
        }
    }
    public void RegisterWaiter(ITurnWaiter waiter)
    {
        if (waiter == null) return;
        turnWaiters.Add(waiter);
    }
    public void UnregisterWaiter(ITurnWaiter waiter)
    {
        if (waiter != null)
        {
            for (int i = 0; i < turnWaiters.Count; ++i)
            {
                if (turnWaiters[i] == waiter)
                {
                    turnWaiters[i] = null;
                    break;
                }
            }
        }
    }
    public void PushEvent(Action action)
    {
        if (action != null) turnEventQueue.Enqueue(action);
    }
    public bool IsAnyOneActing()
    {
        for (int i = turnTakers.Count - 1; i >= 0; --i)
        {
            var taker = turnTakers[i];
            if (taker == null) continue;
            if (taker.IsActing) return true;
        }
        return false;
    }
    public bool NeedWait()
    {
        for (int i = turnWaiters.Count - 1; i >= 0; --i)
        {
            var waiter = turnWaiters[i];
            if (waiter == null) continue;

            if (waiter.Wait) return true;
        }

        return false;
    }
    private IEnumerator Turning()
    {
        while (true)
        {
            turnWaiters.RemoveAll(item => item == null);
            turnTakers.RemoveAll(item => item == null);

            int actedTakerCount = 0;
            for (int i = 0; i < turnTakers.Count; ++i)
            {
                var taker = turnTakers[i];
                if (taker == null) continue;
                if (!taker.CanAct) continue;

                taker.ConsumeEnergy();
                taker.TakeTurn();

                actedTakerCount++;
                if (taker.NeedWait)
                {
                    while (taker.IsActing) yield return null;
                }
            }

            while (true)
            {
                while (IsAnyOneActing()) yield return null;
                
                if (turnEventQueue.Count == 0) break;

                while(turnEventQueue.Count > 0)
                {
                    var action = turnEventQueue.Dequeue();
                    if (action == null) continue;
                    action.Invoke();
                }
            }

            while (NeedWait()) yield return null;

            if (actedTakerCount == 0)
            {
                for (int i = turnTakers.Count - 1; i >= 0; --i)
                {
                    var taker = turnTakers[i];
                    if (taker == null) continue;
                    taker.RecoveryEnergy();
                }
            }

            OnTurnEnd?.Invoke();

            yield return null;
        }
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        StartCoroutine(Turning());
    }
}