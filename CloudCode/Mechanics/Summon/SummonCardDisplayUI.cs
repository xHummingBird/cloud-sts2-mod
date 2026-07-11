
using System;
using System.Collections.Generic;
using System.Linq;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
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
            model =>
            {
                if (!TryGetOwner(model, out var owner) || owner == null)
                    return false;

                return owner.GetRelic<OdinMateria>() != null;
            }
        ),

        new(
            "Bahamut_UI",
            "res://Cloud/scenes/SummonCardDisplay_Bahamut.tscn",
            model =>
            {
                if (!TryGetOwner(model, out var owner) || owner == null)
                    return false;

                return owner.GetRelic<BahamutMateria>() != null;
            }
        )
    };

    public static void EnsureAndRefresh(NCard cardNode)
    {
        if (cardNode == null)
            return;

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
        var result = new List<IconConfig>();

        foreach (var icon in Icons)
        {
            bool shouldShow;

            try
            {
                shouldShow = icon.ShouldShow(model);
            }
            catch (Exception ex)
            {
                GD.PushWarning($"[Cloud Summon Card UI] ShouldShow failed for {icon.Name}: {ex}");
                shouldShow = false;
            }

            if (shouldShow)
                result.Add(icon);
        }

        return result;
    }

    private static void EnsureSingleIcon(Control body, IconConfig config, Vector2 position)
    {
        if (body == null || config == null)
            return;

        var node = body.GetNodeOrNull<Control>(config.Name);

        if (node == null)
        {
            var scene = GetScene(config.Scene);

            if (scene == null)
            {
                GD.PushError($"[Cloud Summon Card UI] Failed to load {config.Scene}");
                return;
            }

            node = scene.Instantiate<Control>();
            node.Name = config.Name;

            // Visual only
            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
            body.MoveChild(node, body.GetChildCount() - 1);

            // Same safe z style as ATB / Limit visual UI
            node.ZIndex = 0;
        }

        node.Visible = true;
        node.Position = position;
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
            GD.PushError($"[Cloud Summon Card UI] Could not load scene: {path}");
        }

        return loaded;
    }

    private static bool TryGetOwner(CardModel model, out Player? owner)
    {
        owner = null;

        if (model == null)
            return false;

        // Compendium / card library cards are canonical.
        // Accessing Owner on canonical models throws CanonicalModelException.
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

#region Hooks

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class SummonDisplayUI_Ready
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
                    SummonCardDisplayUI.EnsureAndRefresh(__instance);
                }
                catch (Exception ex)
                {
                    GD.PushWarning($"[Cloud Summon Card UI] Deferred refresh failed: {ex}");
                }
            }).CallDeferred();
        };

        Callable.From(() =>
        {
            try
            {
                SummonCardDisplayUI.EnsureAndRefresh(__instance);
            }
            catch (Exception ex)
            {
                GD.PushWarning($"[Cloud Summon Card UI] Initial refresh failed: {ex}");
            }
        }).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class SummonDisplayUI_UpdateVisuals
{
    public static void Postfix(NCard __instance)
    {
        if (__instance == null)
            return;

        try
        {
            SummonCardDisplayUI.EnsureAndRefresh(__instance);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"[Cloud Summon Card UI] UpdateVisuals refresh failed: {ex}");
        }
    }
}

#endregion
