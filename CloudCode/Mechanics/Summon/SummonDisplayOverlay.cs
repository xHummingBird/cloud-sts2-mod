using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Cloud.CloudCode.Mechanics.Summon;

public partial class SummonDisplayOverlay : Control
{
    public static SummonDisplayOverlay? Instance { get; private set; }

    private Control? _summonDisplay;
    private RichTextLabel _label;
    private Player _player;

    public override void _Ready()
    {
        Instance = this;
        Name = "SummonDisplayOverlay";

        MouseFilter = MouseFilterEnum.Ignore;

        // ✅ Defer setup (important for stability)
        CallDeferred(nameof(Setup));
    }

    private void Setup()
    {
        if (!IsInsideTree())
            return;
        
        var scene = GD.Load<PackedScene>("res://Cloud/scenes/SummonDisplay.tscn");
        if (scene == null)
            return;

        _summonDisplay = scene.Instantiate<Control>();
        AddChild(_summonDisplay);

        _summonDisplay.MouseFilter = MouseFilterEnum.Ignore;
        _summonDisplay.SetAnchorsPreset(LayoutPreset.BottomLeft);
        _summonDisplay.Position = new Vector2(100, 0);
        _summonDisplay.Visible = true;
        
        _label = _summonDisplay.GetNode<RichTextLabel>("%SummonLabel");
        
        var font = GD.Load<Font>("res://themes/kreon_bold_shared.tres");
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeColorOverride("default_color", Colors.White);
        
        _label.AddThemeFontOverride(
            "normal_font",
            GD.Load<Font>("res://themes/kreon_bold_shared.tres")
        );
        _label.Position += new Vector2(-5, -5);
        _label.AddThemeColorOverride("font_outline_color", new Color(0.2f, 0.2f, 0.2f));
        _label.AddThemeConstantOverride("outline_size", 12);
        _label.AddThemeFontSizeOverride("normal_font_size", 32);
        
        // ✅ Hook player
        var state = CombatManager.Instance.DebugOnlyGetState();
        var player = state?.Players.FirstOrDefault(p => LocalContext.IsMe(p));

        if (player != null)
        {
            SummonManager.GetDataForUI(player).OnSummonChanged += OnSummonChanged;

            UpdateDisplay(SummonManager.GetSummon(player));
        }
    }

    private void OnSummonChanged(int value)
    {
        UpdateDisplay(value);
    }

    private void UpdateDisplay(int value)
    {
        int max = 100;
        
        if (_label != null)
        {
            _label.Text = $"[center]{value}[/center]";
            //change to value after testing
        }

        // ✅ replace this later with label update
    }

    public override void _ExitTree()
    {
        
        if (_player != null)
        {
            var data = SummonManager.GetDataForUI(_player);
            data.OnSummonChanged -= OnSummonChanged;
        }

        // Clear references (defensive)
        _label = null;
        _summonDisplay = null;
        _player = null;

        if (Instance == this)
            Instance = null;

    }
}

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class SummonDisplayOverlayPatch
{
    public static void Postfix(NEnergyCounter __instance)
    {
        if (__instance.GetNodeOrNull("SummonDisplayOverlay") != null)
            return;

        var state = CombatManager.Instance?.DebugOnlyGetState();
        if (state == null) return;
        
        var player = state.Players.FirstOrDefault(p => LocalContext.IsMe(p));
       
        if (player == null) return;
        
        if (!(player.Character is Character.Cloud character))
            return;
        
        var overlay = new SummonDisplayOverlay
        {
            Name = "SummonDisplayOverlay"
        };

        __instance.AddChild(overlay);
    }
}
