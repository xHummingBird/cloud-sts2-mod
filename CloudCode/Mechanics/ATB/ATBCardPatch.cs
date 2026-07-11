
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Cloud.CloudCode.Mechanics.ATB;

public static class ATBCardUi
{
    private const string AtbScenePath = "res://Cloud/scenes/CardATBDisplay.tscn";

    private const string ContainerName = "ATBCostContainer";
    private const string LabelNodeName = "ATBLabel";

    private static PackedScene? _scene;

    private static PackedScene? Scene
    {
        get
        {
            _scene ??= GD.Load<PackedScene>(AtbScenePath);
            return _scene;
        }
    }


    public static void EnsureAndRefresh(NCard cardNode)
    {
        if (!GodotObject.IsInstanceValid(cardNode))
            return;

        // If game/tree is paused, hide ATB overlay.
        // This prevents ATB labels floating over pause/menu overlays.
        if (cardNode.GetTree()?.Paused == true)
        {
            HideIfExists(cardNode);
            return;
        }

        var model = cardNode.Model;

        if (model == null)
        {
            HideIfExists(cardNode);
            return;
        }

        if (model is not IATBCard atbCard)
        {
            HideIfExists(cardNode);
            return;
        }

        var body = cardNode.Body;
        if (body == null)
            return;

        var container = body.GetNodeOrNull<Control>(ContainerName);

        if (container == null)
        {
            var scene = Scene;
            if (scene == null)
                return;

            container = scene.Instantiate<Control>();
            container.Name = ContainerName;
            container.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(container);

            // IMPORTANT:
            // Keep this local to the card.
            // Do NOT use high ZIndex or it can draw above pause/menu overlays.
            container.ZIndex = 0;
            container.ZAsRelative = true;

            body.MoveChild(container, body.GetChildCount() - 1);
        }

        container.Visible = true;
        container.Position = new Vector2(-145f, -205f);

        var label = container.GetNodeOrNull<RichTextLabel>(LabelNodeName);
        if (label == null)
            return;

        label.BbcodeEnabled = true;

        var font = GD.Load<Font>("res://themes/kreon_bold_shared.tres");

        label.AddThemeFontOverride("font", font);
        label.AddThemeFontOverride("normal_font", font);
        label.AddThemeFontSizeOverride("normal_font_size", 24);

        label.AddThemeColorOverride("font_outline_color", new Color(0.2f, 0.2f, 0.2f));
        label.AddThemeConstantOverride("outline_size", 12);

        int displayCost = GetDisplayATBCost(model, atbCard);
        Color color = GetDisplayColor(model, displayCost);

        label.AddThemeColorOverride("default_color", color);
        label.Text = $"[center]{displayCost}[/center]";
    }


    private static int GetDisplayATBCost(CardModel model, IATBCard atbCard)
    {
        bool inCombat = CombatManager.Instance?.IsInProgress ?? false;
        
        if (!inCombat || model.Owner == null || !model.IsMutable)
            return atbCard.ATBCost;
        
        return ATBCostState.GetEffectiveATBCost(model);
    }

    private static Color GetDisplayColor(CardModel model, int displayCost)
    {
        bool inCombat = CombatManager.Instance?.IsInProgress ?? false;
        
        if (!inCombat || model.Owner == null || !model.IsMutable)
            return Colors.White;

        if (displayCost <= 0)
            return Colors.Green;

        int currentATB = ATBManager.GetATB(model.Owner);

        if (currentATB >= displayCost)
            return Colors.White;

        return Colors.Red;
    }

    private static void HideIfExists(NCard cardNode)
    {
        if (!GodotObject.IsInstanceValid(cardNode))
            return;

        var body = cardNode.Body;
        if (body == null)
            return;

        var container = body.GetNodeOrNull<Control>(ContainerName);
        if (container != null)
            container.Visible = false;
    }


    public static async void RefreshDeferred(NCard cardNode)
    {
        if (!GodotObject.IsInstanceValid(cardNode))
            return;

        if (cardNode.GetTree()?.Paused == true)
        {
            HideIfExists(cardNode);
            return;
        }

        EnsureAndRefresh(cardNode);

        var tree = cardNode.GetTree();
        if (tree == null || tree.Paused)
        {
            HideIfExists(cardNode);
            return;
        }

        await cardNode.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

        if (!GodotObject.IsInstanceValid(cardNode))
            return;

        if (cardNode.GetTree()?.Paused == true)
        {
            HideIfExists(cardNode);
            return;
        }

        EnsureAndRefresh(cardNode);

        tree = cardNode.GetTree();
        if (tree == null || tree.Paused)
        {
            HideIfExists(cardNode);
            return;
        }

        await cardNode.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

        if (!GodotObject.IsInstanceValid(cardNode))
            return;

        if (cardNode.GetTree()?.Paused == true)
        {
            HideIfExists(cardNode);
            return;
        }

        EnsureAndRefresh(cardNode);
    }

}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class ATBCardPatch_Ready
{
    public static void Postfix(NCard __instance)
    {

        __instance.ModelChanged += _ =>
        {
            ATBCardUi.RefreshDeferred(__instance);
        };


        ATBCardUi.RefreshDeferred(__instance);
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class ATBCardPatch_UpdateVisuals
{
    public static void Postfix(NCard __instance)
    {

        ATBCardUi.RefreshDeferred(__instance);
    }
}


[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
public static class CardModel_SpendResources_ATB
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref Task<(int, int)> __result)
    {
        if (__instance is not IATBCard)
            return;

        if (!__instance.IsMutable)
            return;

        __result = SpendATBAfterOriginal(__instance, __result);
    }

    private static async Task<(int, int)> SpendATBAfterOriginal(
        CardModel card,
        Task<(int, int)> originalTask)
    {
        var result = await originalTask;

        int cost = ATBCostState.GetEffectiveATBCost(card);

        if (cost > 0)
        {
            ATBManager.SpendATB(card.Owner, cost);
        }

        // Clear "free until played" after the card is played
        ATBCostState.ClearThisTurnOrUntilPlayed(card);

        return result;
    }
}

