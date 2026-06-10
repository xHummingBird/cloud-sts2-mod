using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Cards.Basic;
using Cloud.CloudCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Cloud.CloudCode.Extensions;

[HarmonyPatch(typeof(TouchOfOrobas), "GetUpgradedStarterRelic")]
internal static class CloudTouchOfOrobasPatch
{
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is BusterSword)
        {
            __result = ModelDb.Relic<UltimaWeapon>().ToMutable();
        }
    }
}


[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
internal static class LaharlArchaicToothTranscendencePatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<Braver>().Id] = ModelDb.Card<BraverKai>();
    }
}