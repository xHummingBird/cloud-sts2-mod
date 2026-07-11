
using System;
using System.Collections.Generic;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
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
            ShouldShow = _ => true
        },

        new()
        {
            Name = "Meteor_UI",
            Scene = "res://Cloud/scenes/LimitCardDisplay_Meteorain.tscn",
            Position = new Vector2(75f, -165f),
            ShouldShow = _ => true
        },

        new()
        {
            Name = "Ascension_UI",
            Scene = "res://Cloud/scenes/LimitCardDisplay_Ascension.tscn",
            Position = new Vector2(75f, -125f),
            ShouldShow = _ => true
        },

        new()
        {
            Name = "Omnislash_UI",
            Scene = "res://Cloud/scenes/LimitCardDisplay_Omnislash.tscn",
            Position = new Vector2(75f, -85f),

            // IMPORTANT:
            // Do NOT directly access m.Owner here.
            // Compendium/card library cards are canonical and Owner throws.
            ShouldShow = m =>
            {
                if (!TryGetOwner(m, out var owner) || owner == null)
                    return false;

                return owner.GetRelic<UltimaWeapon>() != null;
            }
        }
    };

    public static void EnsureAndRefresh(NCard cardNode)
    {
        if (cardNode == null)
            return;

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
        if (body == null || model == null || config == null)
            return;

        var node = body.GetNodeOrNull<Control>(config.Name);

        bool shouldShow;

        try
        {
            shouldShow = config.ShouldShow(model);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"[Cloud Limit Card UI] ShouldShow failed for {config.Name}: {ex}");
            shouldShow = false;
        }

        if (!shouldShow)
        {
            if (node != null)
                node.Visible = false;

            return;
        }

        if (node == null)
        {
            var scene = GetScene(config.Scene);

            if (scene == null)
            {
                GD.PushError($"[Cloud Limit Card UI] Failed to load {config.Scene}");
                return;
            }

            node = scene.Instantiate<Control>();
            node.Name = config.Name;

            // Visual only.
            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
            body.MoveChild(node, body.GetChildCount() - 1);

            node.ZIndex = 0;
        }

        node.Visible = true;
        node.Position = config.Position;
    }

    private static void HideAll(Control body)
    {
        if (body == null)
            return;

        foreach (var icon in Icons)
        {
            var node = body.GetNodeOrNull<Control>(icon.Name);

            if (node != null)
                node.Visible = false;
        }
    }

    private static PackedScene? GetScene(string path)
    {
        if (Cache.TryGetValue(path, out var cachedScene))
            return cachedScene;

        var loaded = GD.Load<PackedScene>(path);

        if (loaded != null)
        {
            Cache[path] = loaded;
        }
        else
        {
            GD.PushError($"[Cloud Limit Card UI] Could not load scene: {path}");
        }

        return loaded;
    }

    private static bool TryGetOwner(CardModel model, out Player? owner)
    {
        owner = null;

        if (model == null)
            return false;

        // Canonical / compendium models are not runtime cards.
        // Accessing Owner on them throws CanonicalModelException.
        if (!model.IsMutable)
            return false;

        try
        {
            owner = model.Owner;
            return owner != null;
        }
        catch
        {
            return false;
        }
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class LimitDisplayUI_Ready
{
    public static void Postfix(NCard __instance)
    {
        if (__instance == null)
            return;

        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
            {
                try
                {
                    LimitCardDisplayUI.EnsureAndRefresh(__instance);
                }
                catch (Exception ex)
                {
                    GD.PushWarning($"[Cloud Limit Card UI] Deferred refresh failed: {ex}");
                }
            }).CallDeferred();
        };

        Callable.From(() =>
        {
            try
            {
                LimitCardDisplayUI.EnsureAndRefresh(__instance);
            }
            catch (Exception ex)
            {
                GD.PushWarning($"[Cloud Limit Card UI] Initial refresh failed: {ex}");
            }
        }).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class LimitDisplayUI_UpdateVisuals
{
    public static void Postfix(NCard __instance)
    {
        if (__instance == null)
            return;

        try
        {
            LimitCardDisplayUI.EnsureAndRefresh(__instance);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"[Cloud Limit Card UI] UpdateVisuals refresh failed: {ex}");
        }
    }
}
