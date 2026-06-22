
using System;
using System.Collections.Generic;
using System.Linq;
using Cloud.CloudCode.Cards.Ancient;   // change if your summon cards are elsewhere
using Cloud.CloudCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Cloud.CloudCode.Mechanics.Summon;

public static class SummonCardPatch
{
    private sealed class IconConfig
    {
        public string ContainerName { get; }
        public string ScenePath { get; }
        public Func<CardModel, IHoverTip> HoverTipFactory { get; }
        public Func<CardModel, bool> ShouldShow { get; }

        public IconConfig(
            string containerName,
            string scenePath,
            Func<CardModel, IHoverTip> hoverTipFactory,
            Func<CardModel, bool>? shouldShow = null)
        {
            ContainerName = containerName;
            ScenePath = scenePath;
            HoverTipFactory = hoverTipFactory;
            ShouldShow = shouldShow ?? (_ => true);
        }
    }

    private static readonly Dictionary<string, PackedScene> SceneCache = new();

    // Base slot position + spacing
    private static readonly Vector2 BasePosition = new Vector2(225f, 8f);
    private const float SlotSpacingY = 40f;

    // IMPORTANT:
    // Order here defines slot priority.
    // Odin before Bahamut means:
    // - Odin always takes slot 4 if present
    // - Bahamut falls into slot 4 if Odin absent, or slot 5 if Odin present
    private static readonly IconConfig[] Icons =
    {
        new(
            "IfritIconContainer",
            "res://Cloud/scenes/SummonCardDisplay_Ifrit.tscn",
            model => HoverTipFactory.FromCard<Ifrit>()
        ),

        new(
            "ShivaIconContainer",
            "res://Cloud/scenes/SummonCardDisplay_Shiva.tscn",
            model => HoverTipFactory.FromCard<Shiva>()
        ),

        new(
            "RamuhIconContainer",
            "res://Cloud/scenes/SummonCardDisplay_Ramuh.tscn",
            model => HoverTipFactory.FromCard<Ramuh>()
        ),

        new(
            "OdinIconContainer",
            "res://Cloud/scenes/SummonCardDisplay_Odin.tscn",
            model => HoverTipFactory.FromCard<Odin>(),
            model => model.Owner?.GetRelic<OdinMateria>() != null
        ),

        new(
            "BahamutIconContainer",
            "res://Cloud/scenes/SummonCardDisplay_Bahamut.tscn",
            model => HoverTipFactory.FromCard<Bahamut>(),
            model => model.Owner?.GetRelic<BahamutMateria>() != null
        )
    };

    public static void EnsureAndRefresh(NHandCardHolder holder)
    {
        var model = holder.CardNode?.Model;
        var hitbox = holder.Hitbox;

        if (hitbox == null)
            return;

        // Replace this with your actual "show summon icons on this card" condition
        if (model is not SummonCard)
        {
            HideAll(hitbox);
            return;
        }

        var visibleIcons = GetVisibleIcons(model);

        // Hide everything first, then show only visible ordered icons
        HideAll(hitbox);

        for (int i = 0; i < visibleIcons.Count; i++)
        {
            var config = visibleIcons[i];
            var position = BasePosition + new Vector2(0f, SlotSpacingY * i);
            EnsureSingleIcon(holder, hitbox, config, position);
        }
    }

    private static List<IconConfig> GetVisibleIcons(CardModel model)
    {
        return Icons.Where(icon => icon.ShouldShow(model)).ToList();
    }

    private static void EnsureSingleIcon(
        NHandCardHolder holder,
        Control hitbox,
        IconConfig config,
        Vector2 position)
    {
        var container = hitbox.GetNodeOrNull<Control>(config.ContainerName);

        if (container == null)
        {
            var scene = GetScene(config.ScenePath);
            if (scene == null)
                return;

            container = scene.Instantiate<Control>();
            container.Name = config.ContainerName;

            // Interactive hover layer (your working logic)
            container.MouseFilter = Control.MouseFilterEnum.Pass;

            hitbox.AddChild(container);
            hitbox.MoveChild(container, hitbox.GetChildCount() - 1);

            SetChildControlsToIgnore(container);

            var capturedContainer = container;
            var capturedHolder = holder;
            var capturedConfig = config;

            container.MouseEntered += () =>
                OnHovered(capturedContainer, capturedHolder, capturedConfig);

            container.MouseExited += () =>
                OnUnhovered(capturedHolder);
        }

        container.Visible = true;
        container.Position = position;
    }

    private static void OnHovered(Control owner, NHandCardHolder holder, IconConfig config)
    {
        var model = holder.CardNode?.Model;
        if (model == null)
            return;

        var card = holder.CardNode;

        var tip = NHoverTipSet.CreateAndShow(card, config.HoverTipFactory(model));

        if (tip != null)
        {
            tip.MouseFilter = Control.MouseFilterEnum.Ignore;
            tip.GlobalPosition = card.GlobalPosition + new Vector2(-400f, -200f);
        }
    }

    private static void OnUnhovered(NHandCardHolder holder)
    {
        var card = holder.CardNode;
        if (card != null)
            NHoverTipSet.Remove(card);
    }

    private static void HideAll(Control hitbox)
    {
        foreach (var config in Icons)
        {
            var container = hitbox.GetNodeOrNull<Control>(config.ContainerName);
            if (container != null)
            {
                container.Visible = false;
                NHoverTipSet.Remove(container);
            }
        }
    }

    private static void SetChildControlsToIgnore(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Control c)
                c.MouseFilter = Control.MouseFilterEnum.Ignore;

            SetChildControlsToIgnore(child);
        }
    }

    private static PackedScene? GetScene(string path)
    {
        if (SceneCache.TryGetValue(path, out var cached))
            return cached;

        var scene = GD.Load<PackedScene>(path);
        if (scene != null)
            SceneCache[path] = scene;

        return scene;
    }
}

#region Hooks

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder._Ready))]
public static class SummonCardPatch_Ready
{
    public static void Postfix(NHandCardHolder __instance)
    {
        Callable.From(() => SummonCardPatch.EnsureAndRefresh(__instance)).CallDeferred();
    }
}

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard))]
public static class SummonCardPatch_UpdateCard
{
    public static void Postfix(NHandCardHolder __instance)
    {
        SummonCardPatch.EnsureAndRefresh(__instance);
    }
}

#endregion
