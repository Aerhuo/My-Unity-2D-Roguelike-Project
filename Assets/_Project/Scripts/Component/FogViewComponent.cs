using System;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(HealthComponent))]


[RequireComponent(typeof(GridTransform))]
[RequireComponent(typeof(FOVComponent))]
public class FogViewComponent : MonoBehaviour, IEntitySpawnAndDie, IRefresher
{
    private int viewRadius;
    private GridTransform gridTransform;
    private void UpdateView()
    {
        foreach (var (x, y) in visibleGrids)
        {
            FogManager.Instance.SetFogStateAt(x, y, FogState.Explored);
        }
        visibleGrids.Clear();
    }
    private readonly HashSet<(int, int)> visibleGrids = new();
    private void OnView(int x, int y)
    {
        visibleGrids.Add((x, y));
        FogManager.Instance.SetFogStateAt(x, y, FogState.Visible);
    }
    private FOVComponent fOVComponent;
    private void Awake()
    {
        gridTransform = GetComponent<GridTransform>();
        viewRadius = GetComponent<HealthComponent>().DataSO.viewRadius;
        fOVComponent = GetComponent<FOVComponent>();
    }
    public void OnSpawn()
    {
        Init();
        fOVComponent.OnView += OnView;
        fOVComponent.OnEarlyUpdate += UpdateView;
    }
    private void Init()
    {
        visibleGrids.Clear();
    }
    public void OnDie()
    {
        fOVComponent.OnView -= OnView;
        fOVComponent.OnEarlyUpdate -= UpdateView;
    }

    public void Refresh()
    {
        Init();
    }
}