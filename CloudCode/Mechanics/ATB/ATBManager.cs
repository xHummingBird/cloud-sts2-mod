using MegaCrit.Sts2.Core.Entities.Players;

namespace Cloud.CloudCode.Mechanics;

public class ATBManager

{
    public class ATBData
    {
        public int Value;
        public Action<int>? OnATBChanged;
    }

    private static readonly Dictionary<Player, ATBData> _data = new();

    private static ATBData GetData(Player player)
    {
        if (!_data.TryGetValue(player, out var data))
        {
            data = new ATBData
            {
                Value = 0
            };
            _data[player] = data;
        }
        return data;
    }

    public static int GetATB(Player player)
    {
        return GetData(player).Value;
    }

    public static void SetATB(Player player, int value)
    {
        var data = GetData(player);

        value = Math.Max(0, value);

        if (data.Value == value)
            return;

        data.Value = value;
        data.OnATBChanged?.Invoke(value);
    }

    public static void GainATB(Player player, int amount)
    {
        var current = GetATB(player);

        // ✅ soft cap (your design)
        if (current >= 3)
            return;

        int final = Math.Min(current + amount, 3);
        SetATB(player, final);
    }

    public static void SpendATB(Player player, int amount)
    {
        int current = GetATB(player);
        SetATB(player, current - amount);
    }

    public static ATBData GetDataForUI(Player player)
    {
        return GetData(player);
    }

    public static void Reset(Player player)
    {
        SetATB(player, 0);
    }
}
