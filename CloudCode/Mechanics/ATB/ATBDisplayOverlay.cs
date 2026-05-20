using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Cloud.CloudCode.Mechanics;




public partial class ATBDisplayOverlay : Control
{
    public static ATBDisplayOverlay? Instance { get; private set; }

    private Control? _atbDisplay;

    public override void _Ready()
    {
        Instance = this;
        Name = "ATBDisplayOverlay";

        MouseFilter = MouseFilterEnum.Ignore;

        // ✅ Defer setup (important for stability)
        CallDeferred(nameof(Setup));
    }
    
    private RichTextLabel _label;
    private Player _player;
    private void Setup()
    {
        var scene = GD.Load<PackedScene>("res://Cloud/scenes/ATBDisplay.tscn");
        if (scene == null)
            return;

        _atbDisplay = scene.Instantiate<Control>();
        
        AddChild(_atbDisplay);

        _atbDisplay.MouseFilter = MouseFilterEnum.Stop;
        _atbDisplay.SetAnchorsPreset(LayoutPreset.BottomLeft);
        _atbDisplay.Position = new Vector2(-50, -40);
        _atbDisplay.Visible = true;
        
        _label = _atbDisplay.GetNode<RichTextLabel>("%ATBLabel");
        
        var font = GD.Load<Font>("res://themes/kreon_bold_shared.tres");
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeColorOverride("default_color", Colors.White);
        
        _label.AddThemeFontOverride(
            "normal_font",
            GD.Load<Font>("res://themes/kreon_bold_shared.tres")
        );

        _label.AddThemeColorOverride("font_outline_color", new Color(0.2f, 0.2f, 0.2f));
        _label.AddThemeConstantOverride("outline_size", 14);
        _label.AddThemeFontSizeOverride("normal_font_size", 28);    
        
        // ✅ Hook player
        var state = CombatManager.Instance.DebugOnlyGetState();
        _player = state?.Players.FirstOrDefault(p => LocalContext.IsMe(p));

        
        if (_player != null)
        {
            var data = ATBManager.GetDataForUI(_player);

            data.OnATBChanged += OnATBChanged;

            // ✅ initial sync
            UpdateDisplay(ATBManager.GetATB(_player));
        }

    }

    private void OnATBChanged(int value)
    {
        UpdateDisplay(value);
    }

    private void UpdateDisplay(int value)
    {
        int max = 3;
        
        
        if (_label != null)
        {
            _label.Text = $"[center]{value}/{max}[/center]";
        }

        // ✅ replace this later with label update
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class ATBDisplayOverlayPatch
{
    public static void Postfix(NEnergyCounter __instance)
    {
        if (__instance.GetNodeOrNull("ATBDisplayOverlay") != null)
            return;

        var overlay = new ATBDisplayOverlay
        {
            Name = "ATBDisplayOverlay"
        };

        __instance.AddChild(overlay);
    }
}




