
using System;
using System.Collections.Generic;
using System.Linq;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Cloud.CloudCode.Mechanics.Summon;

public static class SummonCardDisplayUI
{
    private sealed class IconConfig
    {
        public string Name { get; }
        public string Scene { get; }
        public Func<CardModel, bool> ShouldShow { get; }

        public IconConfig(
            string name,
            string scene,
            Func<CardModel, bool>? shouldShow = null)
        {
            Name = name;
            Scene = scene;
            ShouldShow = shouldShow ?? (_ => true);
        }
    }

    private static readonly Dictionary<string, PackedScene> Cache = new();

    // Base slot position + spacing for VISUAL copy (Body space)
    private static readonly Vector2 BasePosition = new Vector2(75f, -205f);
    private const float SlotSpacingY = 40f;

    // Order matters:
    // Odin before Bahamut means Odin always takes slot 4 if available
    private static readonly IconConfig[] Icons =
    {
        new(
            "Ifrit_UI",
            "res://Cloud/scenes/SummonCardDisplay_Ifrit.tscn"
        ),

        new(
            "Shiva_UI",
            "res://Cloud/scenes/SummonCardDisplay_Shiva.tscn"
        ),

        new(
            "Ramuh_UI",
            "res://Cloud/scenes/SummonCardDisplay_Ramuh.tscn"
        ),

        new(
            "Odin_UI",
            "res://Cloud/scenes/SummonCardDisplay_Odin.tscn",
            model => model.Owner?.GetRelic<OdinMateria>() != null
        ),

        new(
            "Bahamut_UI",
            "res://Cloud/scenes/SummonCardDisplay_Bahamut.tscn",
            model => model.Owner?.GetRelic<BahamutMateria>() != null
        )
    };

    public static void EnsureAndRefresh(NCard cardNode)
    {
        var model = cardNode.Model;
        var body = cardNode.Body;

        if (model == null || body == null)
            return;

        // Only show these on the card called Summon
        if (model is not SummonCard)
        {
            HideAll(body);
            return;
        }

        var visibleIcons = GetVisibleIcons(model);

        // Hide all first so removed relic icons disappear immediately
        HideAll(body);

        for (int i = 0; i < visibleIcons.Count; i++)
        {
            var icon = visibleIcons[i];
            var position = BasePosition + new Vector2(0f, SlotSpacingY * i);
            EnsureSingleIcon(body, icon, position);
        }
    }

    private static List<IconConfig> GetVisibleIcons(CardModel model)
    {
        return Icons.Where(icon => icon.ShouldShow(model)).ToList();
    }

    private static void EnsureSingleIcon(Control body, IconConfig config, Vector2 position)
    {
        var node = body.GetNodeOrNull<Control>(config.Name);

        if (node == null)
        {
            var scene = GetScene(config.Scene);
            if (scene == null)
                return;

            node = scene.Instantiate<Control>();
            node.Name = config.Name;

            // Visual only
            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
            body.MoveChild(node, body.GetChildCount() - 1);

            // same safe z style as ATB / your other visual UI
            node.ZIndex = 0;
        }

        node.Visible = true;
        node.Position = position;
    }

    private static void HideAll(Control body)
    {
        foreach (var icon in Icons)
        {
            var node = body.GetNodeOrNull<Control>(icon.Name);
            if (node != null)
                node.Visible = false;
        }
    }

    private static PackedScene? GetScene(string path)
    {
        if (Cache.TryGetValue(path, out var s))
            return s;

        var loaded = GD.Load<PackedScene>(path);
        if (loaded != null)
            Cache[path] = loaded;

        return loaded;
    }
}

#region Hooks

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class SummonDisplayUI_Ready
{
    public static void Postfix(NCard __instance)
    {
        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
                SummonCardDisplayUI.EnsureAndRefresh(__instance)
            ).CallDeferred();
        };

        Callable.From(() =>
            SummonCardDisplayUI.EnsureAndRefresh(__instance)
        ).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class SummonDisplayUI_UpdateVisuals
{
    public static void Postfix(NCard __instance)
    {
        SummonCardDisplayUI.EnsureAndRefresh(__instance);
    }
}

#endregion
