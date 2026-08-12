public class PlayerManaBarComponent : BarComponentBase
{
    protected override float targetPercent => entityStats == null ? 0f : entityStats.MpPercent;
    private EntityStats entityStats;
    protected override void BindPlayer()
    {
        GameManager.Instance.Player.Service.TryGet(out entityStats);
    }
}