using Cloud.CloudCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Cloud.CloudCode.Mechanics.Summon;

public static class SummonManager
{
    public class SummonData
    {
        public Action<int>? OnSummonChanged;
    }

    private const int MaxSummon = 100;

    private static readonly Dictionary<Player, SummonData> _data = new();

    private static LimitRelicBase? GetRelic(Player player)
    {
        return player.Relics
            .OfType<LimitRelicBase>()
            .FirstOrDefault();
    }

    private static SummonData GetData(Player player)
    {
        if (!_data.TryGetValue(player, out var data))
        {
            data = new SummonData();
            _data[player] = data;
        }

        return data;
    }

    public static int GetSummon(Player player)
    {
        return GetRelic(player)?.StoredSummon ?? 0;
    }

    public static void SetSummon(Player player, int value)
    {
        var relic = GetRelic(player);

        if (relic == null)
            return;

        value = Math.Clamp(value, 0, MaxSummon);

        if (relic.StoredSummon == value)
            return;

        relic.StoredSummon = value;

        GetData(player).OnSummonChanged?.Invoke(value);
    }

    public static void HalfSummon(Player player)
    {
        int current = GetSummon(player);

        if (current <= 0)
            return;

        SetSummon(player, current / 2);
    }

    public static void GainSummon(Player player, int amount)
    {
        SetSummon(player, GetSummon(player) + amount);
    }

    public static void SpendSummon(Player player, int amount)
    {
        SetSummon(player, GetSummon(player) - amount);
    }

    public static bool IsFull(Player player)
    {
        return GetSummon(player) >= MaxSummon;
    }

    public static SummonData GetDataForUI(Player player)
    {
        return GetData(player);
    }

    public static void Reset(Player player)
    {
        SetSummon(player, 0);
    }
}