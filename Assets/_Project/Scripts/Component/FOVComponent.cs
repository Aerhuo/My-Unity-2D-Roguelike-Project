using System;
using UnityEngine;
[RequireComponent(typeof(GridTransform))]

[RequireComponent(typeof(HealthComponent))]
public class FOVComponent : MonoBehaviour, IEntitySpawnAndDie, IRefresher
{
    /// <summary>
    /// 不检测是否超出地图边界
    /// </summary>
    public event Action<int, int> OnView;
    /// <summary>
    /// 检测是否超出地图边界
    /// </summary>
    public event Action<int, int> OnSaveView;
    public event Action OnEarlyUpdate;
    public event Action OnLateUpdate;
    private GridTransform gridTransform;
    private Func<int, int, bool> OnStep;
    public int ViewRadius { get; private set; }
    private bool OnSteping(int x, int y)
    {
        bool canContinue = !MapManager.Instance.IsOutOfBounds(x, y) && !MapManager.Instance.IsWall(x, y);

        OnView?.Invoke(x, y);
        if (canContinue) OnSaveView?.Invoke(x, y);
        return canContinue;
    }
    public void UpdateView()
    {
        OnEarlyUpdate?.Invoke();
        Bresenham.CastCircle(gridTransform.Pos, ViewRadius, OnStep);
        OnLateUpdate?.Invoke();
    }
    private void Awake()
    {
        OnStep = OnSteping;
        gridTransform = GetComponent<GridTransform>();
        ViewRadius = GetComponent<HealthComponent>().DataSO.viewRadius;
    }

    public void OnSpawnLate()
    {
        UpdateView();
    }

    public void OnDie()
    {
    }

    public void RefreshLate()
    {
        UpdateView();
    }
}