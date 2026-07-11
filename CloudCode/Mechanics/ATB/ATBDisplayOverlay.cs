
using System;
using System.Linq;
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
    private RichTextLabel? _label;
    private Player? _player;
    private IHoverTip? _hoverTip;

    private int _lastValue = -1;
    private Tween? _popTween;
    private bool _exiting;

    private static readonly Color AtbGainGreen = new Color(0.4f, 1f, 0.4f);

    public override void _Ready()
    {
        Instance = this;
        Name = "ATBDisplayOverlay";

        MouseFilter = MouseFilterEnum.Pass;

        // Defer setup so NEnergyCounter and combat UI can finish entering tree.
        CallDeferred(nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        // Wait for CombatManager / LocalContext / local player to become valid.
        // This is the part that avoids the race condition.
        for (int i = 0; i < 60; i++)
        {
            if (_exiting || !IsInsideTree())
                return;

            var state = CombatManager.Instance?.DebugOnlyGetState();
            var player = state?.Players.FirstOrDefault(p => LocalContext.IsMe(p));

            if (player != null)
            {
                // Not Cloud? Delete the EMPTY overlay node.
                // No visible ATB scene has been created yet, so there is no flash.
                if (player.Character is not Character.Cloud)
                {
                    QueueFree();
                    return;
                }

                _player = player;
                break;
            }

            var tree = GetTree();
            if (tree == null)
                return;

            await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        if (_exiting || !IsInsideTree())
            return;

        var scene = GD.Load<PackedScene>("res://Cloud/scenes/ATBDisplay.tscn");
        if (scene == null)
        {
            GD.PushError("[Cloud ATB] Failed to load res://Cloud/scenes/ATBDisplay.tscn");
            QueueFree();
            return;
        }

        _atbDisplay = scene.Instantiate<Control>();
        AddChild(_atbDisplay);

        _atbDisplay.MouseFilter = MouseFilterEnum.Ignore;
        _atbDisplay.SetAnchorsPreset(LayoutPreset.BottomLeft);
        _atbDisplay.Position = new Vector2(-50, -40);
        _atbDisplay.Visible = true;

        _label = _atbDisplay.GetNodeOrNull<RichTextLabel>("%ATBLabel");

        if (_label == null)
        {
            GD.PushError("[Cloud ATB] Could not find %ATBLabel in ATBDisplay.tscn");
            QueueFree();
            return;
        }

        _label.TreeExiting += () =>
        {
            _popTween?.Kill();
            _popTween = null;
            _label = null;
        };

        var font = GD.Load<Font>("res://themes/kreon_bold_shared.tres");

        if (font != null)
        {
            _label.AddThemeFontOverride("font", font);
            _label.AddThemeFontOverride("normal_font", font);
        }
        else
        {
            GD.PushWarning("[Cloud ATB] Failed to load res://themes/kreon_bold_shared.tres");
        }

        _label.AddThemeColorOverride("default_color", Colors.White);
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

        var data = ATBManager.GetDataForUI(_player);
        data.OnATBChanged += OnATBChanged;
        data.OnMaxATBChanged += OnMaxATBChanged;

        UpdateDisplay(ATBManager.GetATB(_player));
    }

    private void PlayGainPop()
    {
        if (_exiting)
            return;

        var label = _label;

        if (label == null)
            return;

        if (!GodotObject.IsInstanceValid(label) || label.IsQueuedForDeletion())
            return;

        if (_popTween != null && GodotObject.IsInstanceValid(_popTween))
            _popTween.Kill();

        label.Scale = Vector2.One;
        label.Modulate = AtbGainGreen;

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
        if (_hoverTip == null)
            return;

        if (_exiting)
            return;

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

    private void OnMaxATBChanged(int _)
    {
        var player = _player;

        if (player == null)
            return;

        UpdateDisplay(ATBManager.GetATB(player));
    }

    private void UpdateDisplay(int value)
    {
        if (_exiting)
            return;

        var player = _player;
        var label = _label;

        if (player == null)
            return;

        if (label == null)
            return;

        if (!GodotObject.IsInstanceValid(label) || label.IsQueuedForDeletion())
            return;

        int max = ATBManager.GetMaxATB(player);

        try
        {
            label.Text = $"[center]{value}/{max}[/center]";
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        if (_lastValue >= 0 && value > _lastValue)
            PlayGainPop();

        _lastValue = value;
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

        NHoverTipSet.Remove(this);

        _label = null;
        _atbDisplay = null;
        _player = null;
        _hoverTip = null;

        if (Instance == this)
            Instance = null;
    }
}

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class ATBDisplayOverlayPatch
{
    public static void Postfix(NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(__instance) || __instance.IsQueuedForDeletion())
            return;

        if (__instance.GetNodeOrNull<ATBDisplayOverlay>("ATBDisplayOverlay") != null)
            return;

        var overlay = new ATBDisplayOverlay
        {
            Name = "ATBDisplayOverlay"
        };

        __instance.AddChild(overlay);
    }
}
