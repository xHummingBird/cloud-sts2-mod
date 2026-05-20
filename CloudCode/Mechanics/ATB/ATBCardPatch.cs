using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Cloud.CloudCode.Mechanics;


public static class ATBCardUi
{
    // Where your scene lives
    private const string AtbScenePath = "res://Cloud/scenes/CardATBDisplay.tscn";

    // Node name we attach under %OverlayContainer
    private const string ContainerName = "ATBCostContainer";

    // Child names inside CardAtbDisplay.tscn
    private const string LabelNodeName = "ATBLabel";

    // Position tweak relative to EnergyIcon (top-left of the energy gem)

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
        var model = cardNode.Model;
        if (model == null)
            return;

        // Only show ATB UI for ATB cards
        if (model is not IATBCard atbCard)
        {
            HideIfExists(cardNode);
            return;
        }

        // Find overlay container on the card
        
        var body = cardNode.Body;
        if (body == null)
            return;

// Create or reuse our container
        var container = body.GetNodeOrNull<Control>(ContainerName);


        // Create or reuse our container (instantiated from tscn)
        if (container == null)
        {
            var scene = Scene;
            if (scene == null)
                return;

            container = scene.Instantiate<Control>();
            container.Name = ContainerName;
            
            container.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            body.AddChild(container);
            
// ✅ Place it just above base visuals (same layer as glow effects)
            body.MoveChild(container, body.GetChildCount() - 1);

// ✅ No need for ZIndex hacks anymore
            container.ZIndex = 0;


        }

        container.Visible = true;

        // Position: anchor it relative to the EnergyIcon node, but safely in correct coordinate space
        
            // Fallback if EnergyIcon path changes
            container.Position = new Vector2(-145f, -205f);
        
        

        // Update label text
        var label = container.GetNodeOrNull<RichTextLabel>(LabelNodeName);
        var font = GD.Load<Font>("res://themes/kreon_bold_shared.tres");
        label.AddThemeFontOverride("font", font);
        label.AddThemeColorOverride("default_color", Colors.White);
        
        label.AddThemeFontOverride(
            "normal_font",
            GD.Load<Font>("res://themes/kreon_bold_shared.tres")
        );

        label.AddThemeColorOverride("font_outline_color", new Color(0.2f, 0.2f, 0.2f));
        label.AddThemeConstantOverride("outline_size", 12);
        label.AddThemeFontSizeOverride("normal_font_size", 24);
        
        if (label != null)
        {
            // Ensure BBCode is on (in case you forgot in the scene)
            label.BbcodeEnabled = true;

            // Cost only (you can change this to "x/3" if you want)
            label.Text = $"[center]{atbCard.ATBCost}[/center]";
        }
    }

    private static void HideIfExists(NCard cardNode)
    {
        
        var body = cardNode.Body;
        if (false)
            return;

        var container = body.GetNodeOrNull<Control>(ContainerName);
        if (container != null)
            container.Visible = false;
    }
}

//////////////////////////////////////////////////////////////
// PATCH 1: Subscribe once per NCard instance (pool-safe)
//////////////////////////////////////////////////////////////

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class ATBCardPatch_Ready
{
    public static void Postfix(NCard __instance)
    {
        // When Model is assigned/reassigned (pooling), refresh
        __instance.ModelChanged += _ =>
        {
            Callable.From(() => ATBCardUi.EnsureAndRefresh(__instance)).CallDeferred();
        };

        // Initial refresh (in case model already exists by the time _Ready runs)
        Callable.From(() => ATBCardUi.EnsureAndRefresh(__instance)).CallDeferred();
    }
}

//////////////////////////////////////////////////////////////
// PATCH 2: Refresh whenever visuals update (hand/preview changes)
//////////////////////////////////////////////////////////////

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class ATBCardPatch_UpdateVisuals
{
    public static void Postfix(NCard __instance)
    {
        ATBCardUi.EnsureAndRefresh(__instance);
    }
}



