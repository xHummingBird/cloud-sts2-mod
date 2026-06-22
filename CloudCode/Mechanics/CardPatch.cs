using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Stance;
using Cloud.CloudCode.Mechanics.Summon;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Cloud.CloudCode.Mechanics;


[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class CardUI_Update
{
    public static void Postfix(NCard __instance)
    {
        MagicSummonCardDisplayUI.Ensure(__instance);
        OperatorCardDisplayUI.Ensure(__instance);
        StanceSwitchDisplayUI.Ensure(__instance);
        PunisherCardDisplayUI.Ensure(__instance);
    }
}


[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class CardUI_Ready
{
    public static void Postfix(NCard __instance)
    {
        Callable.From(() =>
        {
            MagicSummonCardDisplayUI.Ensure(__instance);
            OperatorCardDisplayUI.Ensure(__instance);
            StanceSwitchDisplayUI.Ensure(__instance);
            PunisherCardDisplayUI.Ensure(__instance);
        }).CallDeferred();
    }
}


[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class CardUI_ModelChanged
{
    public static void Postfix(NCard __instance)
    {
        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
            {
                MagicSummonCardDisplayUI.Ensure(__instance);
                OperatorCardDisplayUI.Ensure(__instance);
                StanceSwitchDisplayUI.Ensure(__instance);
                PunisherCardDisplayUI.Ensure(__instance);

                // ✅ second pass = fixes reward + sorting glitches
                Callable.From(() =>
                {
                    MagicSummonCardDisplayUI.Ensure(__instance);
                    OperatorCardDisplayUI.Ensure(__instance);
                    StanceSwitchDisplayUI.Ensure(__instance);
                    PunisherCardDisplayUI.Ensure(__instance);
                }).CallDeferred();

            }).CallDeferred();
        };
    }
}




