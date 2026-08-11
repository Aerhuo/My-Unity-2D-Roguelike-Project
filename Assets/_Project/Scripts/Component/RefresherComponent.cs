using System.Collections.Generic;
using UnityEngine;

public class RefresherComponent : MonoBehaviour
{
    public void RefreshComponents()
    {
        foreach (var refresh in refreshers) refresh.Refresh();
        foreach (var refresh in refreshers) refresh.RefreshLate();
    }
    private readonly List<IRefresher> refreshers = new(10);
    private void Awake()
    {
        GetComponents(refreshers);
    }
}