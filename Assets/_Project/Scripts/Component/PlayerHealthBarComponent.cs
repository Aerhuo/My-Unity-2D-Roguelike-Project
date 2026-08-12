using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarComponent : BarComponentBase
{
    protected override float targetPercent => healthComponent == null ? 0f : healthComponent.HpPercent;
    private HealthComponent healthComponent;
    protected override void BindPlayer()
    {
        GameManager.Instance.Player.Service.TryGet(out healthComponent);
    }
}