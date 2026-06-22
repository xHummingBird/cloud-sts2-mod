
using System;
using System.Collections.Generic;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Cloud.CloudCode.Mechanics.Limit;

public static class LimitCardDisplayUI
{
    private sealed class IconConfig
    {
        public string Name;
        public string Scene;
        public Vector2 Position;
        public Func<CardModel, bool> ShouldShow;
    }

    private static readonly Dictionary<string, PackedScene> Cache = new();

    
    private static readonly IconConfig[] Icons =
    {
        new()
        {
            Name = "CrossSlash_UI",
            Scene = "res://Cloud/scenes/LimitCardDisplay_CrossSlash.tscn",
            Position = new Vector2(75f, -205f),
            ShouldShow = m => true
        },

        new()
        {
            Name = "Meteor_UI",
            Scene = "res://Cloud/scenes/LimitCardDisplay_Meteorain.tscn",
            Position = new Vector2(75f, -165f),
            ShouldShow = m => true
        },

        new()
        {
            Name = "Ascension_UI",
            Scene = "res://Cloud/scenes/LimitCardDisplay_Ascension.tscn",
            Position = new Vector2(75f, -125f),
            ShouldShow = m => true
        },

        new()
        {
            Name = "Omnislash_UI",
            Scene = "res://Cloud/scenes/LimitCardDisplay_Omnislash.tscn",
            Position = new Vector2(75f, -85f),
            ShouldShow = m => m.Owner?.GetRelic<UltimaWeapon>() != null
        }
    };


    public static void EnsureAndRefresh(NCard cardNode)
    {
        var model = cardNode.Model;
        var body = cardNode.Body;

        if (model == null || body == null)
            return;

        if (model is not LimitBreak)
        {
            HideAll(body);
            return;
        }

        foreach (var icon in Icons)
        {
            EnsureSingleIcon(body, model, icon);
        }
    }

    private static void EnsureSingleIcon(Control body, CardModel model, IconConfig config)
    {
        var node = body.GetNodeOrNull<Control>(config.Name);

        if (!config.ShouldShow(model))
        {
            if (node != null)
                node.Visible = false;
            return;
        }

        if (node == null)
        {
            var scene = GetScene(config.Scene);
            if (scene == null)
                return;

            node = scene.Instantiate<Control>();
            node.Name = config.Name;

            // ✅ VISUAL ONLY
            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
            body.MoveChild(node, body.GetChildCount() - 1);

            node.ZIndex = 0; // ✅ same as ATB (safe)
        }

        node.Visible = true;
        node.Position = config.Position;
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

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class LimitDisplayUI_Ready
{
    public static void Postfix(NCard __instance)
    {
        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
                LimitCardDisplayUI.EnsureAndRefresh(__instance)
            ).CallDeferred();
        };

        Callable.From(() =>
            LimitCardDisplayUI.EnsureAndRefresh(__instance)
        ).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class LimitDisplayUI_UpdateVisuals
{
    public static void Postfix(NCard __instance)
    {
        LimitCardDisplayUI.EnsureAndRefresh(__instance);
    }
}

