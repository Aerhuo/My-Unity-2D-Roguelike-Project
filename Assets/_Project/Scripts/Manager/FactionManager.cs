public static class FactionManager
{
    public static bool IsEnemy(this Faction self, Faction other)
    {
        if (self != other) return true;
        return false;
    }
}