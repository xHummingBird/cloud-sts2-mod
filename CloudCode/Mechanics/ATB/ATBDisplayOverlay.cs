using Cloud.CloudCode.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Cloud.CloudCode.Mechanics.ATB;

public partial class ATBDisplayOverlay : Control
{
    public static ATBDisplayOverlay? Instance { get; private set; }

    private Control? _atbDisplay;
    private int _lastValue = -1;
    private Tween? _popTween;
    private bool _exiting;
    private static readonly Color AtbGainGreen = new Color(0.4f, 1f, 0.4f);
    
    private RichTextLabel _label;
    private Player _player;
    private IHoverTip _hoverTip;
    
    public override void _Ready()
    {
        Instance = this;
        Name = "ATBDisplayOverlay";

        MouseFilter = MouseFilterEnum.Pass;

        // ✅ Defer setup (important for stability)
        CallDeferred(nameof(Setup));
    }
    
    private void Setup()
    {
        if (!IsInsideTree())
            return;
        
        var scene = GD.Load<PackedScene>("res://Cloud/scenes/ATBDisplay.tscn");
        if (scene == null)
            return;

        _atbDisplay = scene.Instantiate<Control>();
        
        AddChild(_atbDisplay);

        _atbDisplay.MouseFilter = MouseFilterEnum.Ignore;
        _atbDisplay.SetAnchorsPreset(LayoutPreset.BottomLeft);
        _atbDisplay.Position = new Vector2(-50, -40);
        _atbDisplay.Visible = true;
        
        _label = _atbDisplay.GetNode<RichTextLabel>("%ATBLabel");
        
        _label.TreeExiting += () =>
        {
            _popTween?.Kill();
            _popTween = null;
            _label = null;
        };
        
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
        
        _hoverTip = CloudStaticHoverTip.ATB;
        
        
        _label.MouseFilter = MouseFilterEnum.Pass;

        _label.Connect(SignalName.MouseEntered, Callable.From(OnHovered));
        _label.Connect(SignalName.MouseExited, Callable.From(OnUnhovered));


        MouseFilter = MouseFilterEnum.Pass;
        Connect(SignalName.MouseEntered, Callable.From(OnHovered));
        Connect(SignalName.MouseExited, Callable.From(OnUnhovered));
        
        // ✅ Hook player
        var state = CombatManager.Instance.DebugOnlyGetState();
        _player = state?.Players.FirstOrDefault(p => LocalContext.IsMe(p));

        
        if (_player != null)
        {
            var data = ATBManager.GetDataForUI(_player);

            data.OnATBChanged += OnATBChanged;
            data.OnMaxATBChanged += OnMaxATBChanged;

            // ✅ initial sync
            UpdateDisplay(ATBManager.GetATB(_player));
        }

    }
    
    
    private void PlayGainPop()
    {
        if (_exiting) return;
        
        var label = _label;
        
        if (label == null) return;

        if (!GodotObject.IsInstanceValid(label) || label.IsQueuedForDeletion())
            return;

        if (_popTween != null && GodotObject.IsInstanceValid(_popTween))
            _popTween.Kill();

        label.Scale = Vector2.One;
        label.Modulate = AtbGainGreen;

        // Bind tween lifetime to the LABEL (the thing we're animating)
        _popTween = label.CreateTween();

        _popTween.TweenProperty(label, "scale", new Vector2(1.25f, 1.25f), 0.10f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _popTween.TweenProperty(label, "scale", Vector2.One, 0.40f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _popTween.Parallel().TweenProperty(label, "modulate", Colors.White, 0.40f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

    }
    
    
    private void OnHovered()
    {
        NHoverTipSet.Clear();
        
        var tip = NHoverTipSet.CreateAndShow(this, _hoverTip);
        tip.GlobalPosition = GlobalPosition + new Vector2(-75f, -475f);
        tip.MouseFilter = MouseFilterEnum.Ignore;
    }

    private void OnUnhovered()
    {
        NHoverTipSet.Remove(this);
    }



    private void OnATBChanged(int value)
    {
        UpdateDisplay(value);
    }

    private void UpdateDisplay(int value)
    {
        if (_exiting) return;
        
        
        var player = _player;
        var label = _label;

        if (player == null) return;
        if (label == null) return;

        // IMPORTANT: also avoid "about to be deleted"
        if (!GodotObject.IsInstanceValid(label) || label.IsQueuedForDeletion())
            return;

        int max = ATBManager.GetMaxATB(player);

        // Setting Text is where your crash happens, so guard hard
        try
        {
            label.Text = $"[center]{value}/{max}[/center]";
        }
        catch (ObjectDisposedException)
        {
            // Godot freed it between our checks (can happen around scene teardown)
            return;
        }

        if (_lastValue >= 0 && value > _lastValue)
            PlayGainPop();

        _lastValue = value;

    }

    
    private void OnMaxATBChanged(int _)
    {
        if (_player == null) return;
        UpdateDisplay(ATBManager.GetATB(_player));
    }
    
    public override void _ExitTree()
    {
        _exiting = true;
        
        if (_popTween != null && GodotObject.IsInstanceValid(_popTween))
            _popTween.Kill();
        _popTween = null;
        
        if (_player != null)
        {
            var data = ATBManager.GetDataForUI(_player);
            data.OnATBChanged -= OnATBChanged;
            data.OnMaxATBChanged -= OnMaxATBChanged;
        }

        // Clear references (defensive)
        _label = null;
        _atbDisplay = null;
        _player = null;

        if (Instance == this)
            Instance = null;

    }
}


[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class ATBDisplayOverlayPatch
{
    public static void Postfix(NEnergyCounter __instance)
    {
        var state = CombatManager.Instance?.DebugOnlyGetState();
        if (state == null) return;

        var player = state.Players.FirstOrDefault(p => LocalContext.IsMe(p));
        if (player == null) return;

        // ✅ CHARACTER CHECK (THIS IS WHAT YOU WANT)
        if (!(player.Character is Character.Cloud character))
            return;

        // ✅ only Cloud reaches here

        if (__instance.GetNodeOrNull("ATBDisplayOverlay") != null)
            return;

        var overlay = new ATBDisplayOverlay
        {
            Name = "ATBDisplayOverlay"
        };
        __instance.AddChild(overlay);
    }
}





