using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Cloud.CloudCode.Mechanics.ATB;

public class ATBManager

{
    public class ATBData
    {
        public int Value;
        public int GainThisTurn;
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
    
    public static void ResetGainThisTurn(Player player)
    {
        var data = GetData(player);
        data.GainThisTurn = 0;
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
    
    
    public static void GainATBFromAttack(Player player, int amount)
    {
        var data = GetData(player);

        int max = 3; // your soft cap
        int current = data.Value;

        // ✅ limit: max gain per turn from attacks = max ATB
        int remainingThisTurn = max - data.GainThisTurn;
        if (remainingThisTurn <= 0)
            return;

        int allowed = Math.Min(amount, remainingThisTurn);

        // ✅ also don't exceed soft cap from this source
        int final = Math.Min(current + allowed, max);

        int actualGain = final - current;
        if (actualGain <= 0)
            return;

        data.GainThisTurn += actualGain;

        SetATB(player, final);
    }
    
    public static void GainATBDirect(Player player, int amount)
    {
        int current = GetATB(player);
        SetATB(player, current + amount);
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
