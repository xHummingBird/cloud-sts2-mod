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
        
        public int BaseMaxATB = 3;
        public int BonusMaxATB = 0;

        public Action<int>? OnATBChanged;
        public Action<int>? OnMaxATBChanged;
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

    
    public static int GetMaxATB(Player player)
    {
        var data = GetData(player);
        return Math.Max(0, data.BaseMaxATB + data.BonusMaxATB);
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
    
    
    public static void AddMaxATB(Player player, int amount)
    {
        if (amount == 0) return;

        var data = GetData(player);
        int oldMax = GetMaxATB(player);

        data.BonusMaxATB += amount;

        int newMax = GetMaxATB(player);
        if (newMax != oldMax)
            data.OnMaxATBChanged?.Invoke(newMax);
    }

    
    
    public static void GainATBFromAttack(Player player, int amount)
    {
        var data = GetData(player);

        int max = GetMaxATB(player);; // your soft cap
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
    
    public static void GainATBDirect(Player? player, int amount)
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

    public static void Reset(Player? player)
    {
        SetATB(player, 0);
        
        var data = GetData(player);
// Reset core values
        data.Value = 0;
        data.GainThisTurn = 0;

        // ✅ Reset max ATB (important)
        data.BaseMaxATB = 3;
        data.BonusMaxATB = 0;

        data.OnATBChanged?.Invoke(0);
        data.OnMaxATBChanged?.Invoke(GetMaxATB(player));

    }
}
