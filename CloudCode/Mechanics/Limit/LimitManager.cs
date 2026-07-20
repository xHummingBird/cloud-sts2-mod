using Cloud.CloudCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Cloud.CloudCode.Mechanics.Limit;

public static class LimitManager
{
    public class LimitData
    {
        public Action<int>? OnLimitChanged;
    }

    private const int MaxLimit = 100;

    private static readonly Dictionary<Player, LimitData> _data = new();

    private static LimitRelicBase? GetRelic(Player player)
    {
        return player.Relics
            .OfType<LimitRelicBase>()
            .FirstOrDefault();
    }

    private static LimitData GetData(Player player)
    {
        if (!_data.TryGetValue(player, out var data))
        {
            data = new LimitData();
            _data[player] = data;
        }

        return data;
    }

    public static int GetLimit(Player player)
    {
        return GetRelic(player)?.StoredLimit ?? 0;
    }

    public static void SetLimit(Player player, int value)
    {
        var relic = GetRelic(player);

        if (relic == null)
            return;

        value = Math.Clamp(value, 0, MaxLimit);

        if (relic.StoredLimit == value)
            return;

        relic.StoredLimit = value;

        GetData(player).OnLimitChanged?.Invoke(value);
    }

    public static void HalfLimit(Player player)
    {
        int current = GetLimit(player);

        if (current <= 0)
            return;

        SetLimit(player, current / 2);
    }

    public static void GainLimit(Player player, int amount)
    {
        SetLimit(player, GetLimit(player) + amount);
    }

    public static void SpendLimit(Player player, int amount)
    {
        SetLimit(player, GetLimit(player) - amount);
    }

    public static bool IsFull(Player player)
    {
        return GetLimit(player) >= MaxLimit;
    }

    public static LimitData GetDataForUI(Player player)
    {
        return GetData(player);
    }

    public static void Reset(Player player)
    {
        SetLimit(player, 0);
    }
}