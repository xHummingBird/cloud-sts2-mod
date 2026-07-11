
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

namespace Cloud.CloudCode.Mechanics.Summon;

public partial class SummonDisplayOverlay : Control
{
    public static SummonDisplayOverlay? Instance { get; private set; }

    private Control? _summonDisplay;
    private RichTextLabel? _label;
    private Player? _player;
    private IHoverTip? _hoverTip;

    private int _lastValue = -1;
    private Tween? _popTween;
    private bool _exiting;

    private const int SummonMax = 100;
    private static readonly Color SummonGainGreen = new Color(0.4f, 1f, 0.4f);

    public override void _Ready()
    {
        Instance = this;
        Name = "SummonDisplayOverlay";

        MouseFilter = MouseFilterEnum.Pass;

        // Defer setup so NEnergyCounter/combat UI can finish entering tree.
        CallDeferred(nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        // Wait for CombatManager / LocalContext / local player.
        // This avoids the race condition from NEnergyCounter._Ready().
        for (int i = 0; i < 60; i++)
        {
            if (_exiting || !IsInsideTree())
                return;

            var state = CombatManager.Instance?.DebugOnlyGetState();
            var player = state?.Players.FirstOrDefault(p => LocalContext.IsMe(p));

            if (player != null)
            {
                // Not Cloud? Delete the EMPTY overlay node.
                // The visible SummonDisplay.tscn has not been created yet.
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

        var scene = GD.Load<PackedScene>("res://Cloud/scenes/SummonDisplay.tscn");
        if (scene == null)
        {
            GD.PushError("[Cloud Summon] Failed to load res://Cloud/scenes/SummonDisplay.tscn");
            QueueFree();
            return;
        }

        _summonDisplay = scene.Instantiate<Control>();
        AddChild(_summonDisplay);

        _summonDisplay.MouseFilter = MouseFilterEnum.Ignore;
        _summonDisplay.SetAnchorsPreset(LayoutPreset.BottomLeft);
        _summonDisplay.Position = new Vector2(100, 0);
        _summonDisplay.Visible = true;

        _label = _summonDisplay.GetNodeOrNull<RichTextLabel>("%SummonLabel");

        if (_label == null)
        {
            GD.PushError("[Cloud Summon] Could not find %SummonLabel in SummonDisplay.tscn");
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
            GD.PushWarning("[Cloud Summon] Failed to load res://themes/kreon_bold_shared.tres");
        }

        _label.AddThemeColorOverride("default_color", Colors.White);
        _label.Position += new Vector2(-5, -5);
        _label.AddThemeColorOverride("font_outline_color", new Color(0.2f, 0.2f, 0.2f));
        _label.AddThemeConstantOverride("outline_size", 12);
        _label.AddThemeFontSizeOverride("normal_font_size", 32);

        _hoverTip = CloudStaticHoverTip.Summon;

        _label.MouseFilter = MouseFilterEnum.Pass;
        _label.Connect(SignalName.MouseEntered, Callable.From(OnHovered));
        _label.Connect(SignalName.MouseExited, Callable.From(OnUnhovered));

        MouseFilter = MouseFilterEnum.Pass;
        Connect(SignalName.MouseEntered, Callable.From(OnHovered));
        Connect(SignalName.MouseExited, Callable.From(OnUnhovered));

        var data = SummonManager.GetDataForUI(_player);
        data.OnSummonChanged += OnSummonChanged;

        UpdateDisplay(SummonManager.GetSummon(_player));
    }

    private void PlayGainPop(bool stayGreenAfter)
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
        label.Modulate = SummonGainGreen;

        _popTween = label.CreateTween();

        _popTween.TweenProperty(label, "scale", new Vector2(1.25f, 1.25f), 0.10f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _popTween.TweenProperty(label, "scale", Vector2.One, 0.40f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _popTween.Parallel().TweenProperty(
                label,
                "modulate",
                stayGreenAfter ? SummonGainGreen : Colors.White,
                0.40f
            )
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
    }

    private void OnHovered()
    {
        if (_exiting)
            return;

        if (_hoverTip == null)
            return;

        NHoverTipSet.Clear();

        var tip = NHoverTipSet.CreateAndShow(this, _hoverTip);
        tip.GlobalPosition = GlobalPosition + new Vector2(-75f, -550f);
        tip.MouseFilter = MouseFilterEnum.Ignore;
    }

    private void OnUnhovered()
    {
        NHoverTipSet.Remove(this);
    }

    private void OnSummonChanged(int value)
    {
        UpdateDisplay(value);
    }

    private void UpdateDisplay(int value)
    {
        if (_exiting)
            return;

        var label = _label;

        if (label == null)
            return;

        if (!GodotObject.IsInstanceValid(label) || label.IsQueuedForDeletion())
            return;

        try
        {
            label.Text = $"[center]{value}[/center]";
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        bool isMaxed = value >= SummonMax;

        if (_lastValue >= 0 && value > _lastValue)
        {
            PlayGainPop(isMaxed);
        }
        else
        {
            label.Scale = Vector2.One;
            label.Modulate = isMaxed ? SummonGainGreen : Colors.White;
        }

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
            var data = SummonManager.GetDataForUI(_player);
            data.OnSummonChanged -= OnSummonChanged;
        }

        NHoverTipSet.Remove(this);

        _label = null;
        _summonDisplay = null;
        _player = null;
        _hoverTip = null;

        if (Instance == this)
            Instance = null;
    }
}

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class SummonDisplayOverlayPatch
{
    public static void Postfix(NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(__instance) || __instance.IsQueuedForDeletion())
            return;

        if (__instance.GetNodeOrNull<SummonDisplayOverlay>("SummonDisplayOverlay") != null)
            return;

        var overlay = new SummonDisplayOverlay
        {
            Name = "SummonDisplayOverlay"
        };

        __instance.AddChild(overlay);
    }
}
