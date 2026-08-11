public static class FactionManager
{
    public static bool IsEnemy(this Faction self, Faction other)
    {
        if (self != other) return true;
        return false;
    }
    public static bool IsEnemy(this Faction self, IEntity entity)
    {
        if (entity.Service.TryGet<IDamageable>(out _) && entity.Service.TryGet<IFaction>(out var faction))
        {
            if (self.IsEnemy(faction.Faction)) return true;
        }

        return false;
    }
    public static bool IsEnemy(this IFaction self, IEntity entity) => IsEnemy(self.Faction, entity);
}