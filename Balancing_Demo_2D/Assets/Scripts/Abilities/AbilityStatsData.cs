/*using Unity.Netcode;

[System.Serializable]
public struct AbilityStatsData : INetworkSerializable
{
    public float damageOverTime;
    public float damage;
    public float costToDamage;
    public float totalManaSpent;
    public float damageTotal;
    public float gameTime;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref damageOverTime);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref costToDamage);
        serializer.SerializeValue(ref totalManaSpent);
        serializer.SerializeValue(ref damageTotal);
        serializer.SerializeValue(ref gameTime);
    }

    public static AbilityStatsData FromAbilityStats(AbilityStats stats)
    {
        return new AbilityStatsData
        {
            damageOverTime = stats.damageOverTime,
            damage = stats.damage,
            costToDamage = stats.costToDamage,
            totalManaSpent = stats.totalManaSpent,
            damageTotal = stats.damageTotal,
            gameTime = stats.gameTime
        };
    }

    public void ApplyTo(AbilityStats stats)
    {
        stats.damageOverTime = damageOverTime;
        stats.damage = damage;
        stats.costToDamage = costToDamage;
        stats.totalManaSpent = totalManaSpent;
        stats.damageTotal = damageTotal;
        stats.gameTime = gameTime;
    }
}*/