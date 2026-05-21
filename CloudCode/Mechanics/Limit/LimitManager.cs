using MegaCrit.Sts2.Core.Entities.Players;

namespace Cloud.CloudCode.Mechanics.Limit;


public static class LimitManager
{
    public class LimitData
    {
        public int Value;
        public Action<int>? OnLimitChanged;
    }

    private static readonly Dictionary<Player, LimitData> _data = new();

    private const int MaxLimit = 100;

    private static LimitData GetData(Player player)
    {
        if (!_data.TryGetValue(player, out var data))
        {
            data = new LimitData
            {
                Value = 0
            };

            _data[player] = data;
        }

        return data;
    }

    public static int GetLimit(Player player)
    {
        return GetData(player).Value;
    }

    public static void SetLimit(Player player, int value)
    {
        var data = GetData(player);

        // ✅ Clamp between 0 and 100
        value = Math.Max(0, Math.Min(value, MaxLimit));

        if (data.Value == value)
            return;

        data.Value = value;

        // ✅ Notify UI
        data.OnLimitChanged?.Invoke(value);
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
