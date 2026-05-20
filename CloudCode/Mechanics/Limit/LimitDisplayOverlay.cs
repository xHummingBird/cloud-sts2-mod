using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Cloud.CloudCode.Mechanics;

public partial class LimitDisplayOverlay : Control
{
    public static LimitDisplayOverlay? Instance { get; private set; }

    private Control? _limitDisplay;
    private RichTextLabel _label;

    public override void _Ready()
    {
        Instance = this;
        Name = "LimitDisplayOverlay";

        MouseFilter = MouseFilterEnum.Ignore;

        // ✅ Defer setup (important for stability)
        CallDeferred(nameof(Setup));
    }

    private void Setup()
    {
        var scene = GD.Load<PackedScene>("res://Cloud/scenes/LimitDisplay.tscn");
        if (scene == null)
            return;

        _limitDisplay = scene.Instantiate<Control>();
        AddChild(_limitDisplay);

        _limitDisplay.MouseFilter = MouseFilterEnum.Ignore;
        _limitDisplay.SetAnchorsPreset(LayoutPreset.BottomLeft);
        _limitDisplay.Position = new Vector2(70, 80);
        _limitDisplay.Visible = true;
        
        _label = _limitDisplay.GetNode<RichTextLabel>("%LimitLabel");
        
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
            LimitManager.GetDataForUI(player).OnLimitChanged += OnLimitChanged;

            UpdateDisplay(LimitManager.GetLimit(player));
        }
    }

    private void OnLimitChanged(int value)
    {
        UpdateDisplay(value);
    }

    private void UpdateDisplay(int value)
    {
        int max = 3;
        
        
        if (_label != null)
        {
            _label.Text = $"[center]{"100"}[/center]";
            //change to value after testing
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
public static class LimitDisplayOverlayPatch
{
    public static void Postfix(NEnergyCounter __instance)
    {
        if (__instance.GetNodeOrNull("LimitDisplayOverlay") != null)
            return;

        var overlay = new LimitDisplayOverlay
        {
            Name = "LimitDisplayOverlay"
        };

        __instance.AddChild(overlay);
    }
}
