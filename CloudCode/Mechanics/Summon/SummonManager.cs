using MegaCrit.Sts2.Core.Entities.Players;

namespace Cloud.CloudCode.Mechanics.Summon;


public static class SummonManager
{
    public class SummonData
    {
        public int Value;
        public Action<int>? OnSummonChanged;
    }

    private static readonly Dictionary<Player, SummonData> _data = new();

    private const int MaxSummon = 100;

    private static SummonData GetData(Player player)
    {
        if (!_data.TryGetValue(player, out var data))
        {
            data = new SummonData
            {
                Value = 0
            };

            _data[player] = data;
        }

        return data;
    }

    public static int GetSummon(Player player)
    {
        return GetData(player).Value;
    }

    public static void SetSummon(Player player, int value)
    {
        var data = GetData(player);

        // ✅ Clamp between 0 and 100
        value = Math.Max(0, Math.Min(value, MaxSummon));

        if (data.Value == value)
            return;

        data.Value = value;

        // ✅ Notify UI
        data.OnSummonChanged?.Invoke(value);
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
