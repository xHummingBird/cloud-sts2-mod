
using System;
using System.Collections.Generic;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Cloud.CloudCode.Mechanics.Limit;

public static class LimitCardPatch
{
    private sealed class IconConfig
    {
        public string ContainerName { get; }
        public string ScenePath { get; }
        public Vector2 Position { get; }
        public Func<CardModel, IHoverTip> HoverTipFactory { get; }
        public Func<CardModel, bool>? ShouldShow { get; }

        public IconConfig(
            string containerName,
            string scenePath,
            Vector2 position,
            Func<CardModel, IHoverTip> hoverTipFactory,
            Func<CardModel, bool>? shouldShow = null)
        {
            ContainerName = containerName;
            ScenePath = scenePath;
            Position = position;
            HoverTipFactory = hoverTipFactory;
            ShouldShow = shouldShow ?? (_ => true);
        }
    }

    private static readonly Dictionary<string, PackedScene> SceneCache = new();

    private static readonly IconConfig[] Icons =
    {
        new(
            "CrossSlashKaiIconContainer",
            "res://Cloud/scenes/LimitCardDisplay_CrossSlash.tscn",
            new Vector2(225f, 8f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<CrossSlashKai>(true)
                : HoverTipFactory.FromCard<CrossSlashKai>()
        ),

        new(
            "MeteorainIconContainer",
            "res://Cloud/scenes/LimitCardDisplay_Meteorain.tscn",
            new Vector2(225f, 48f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<Meteorain>(true)
                : HoverTipFactory.FromCard<Meteorain>()
        ),

        new(
            "AscensionIconContainer",
            "res://Cloud/scenes/LimitCardDisplay_Ascension.tscn",
            new Vector2(225f, 88f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<Ascension>(true)
                : HoverTipFactory.FromCard<Ascension>()
        ),
        
        
        new(
            "OmnislashIconContainer",
            "res://Cloud/scenes/LimitCardDisplay_Omnislash.tscn",
            new Vector2(225f, 128f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<Omnislash>(true)
                : HoverTipFactory.FromCard<Omnislash>(),
            model => model.Owner?.GetRelic<UltimaWeapon>() != null
        )
        
    };

    public static void EnsureAndRefresh(NHandCardHolder holder)
    {
        var model = holder.CardNode?.Model;
        var hitbox = holder.Hitbox;

        if (hitbox == null)
            return;

        if (model is not LimitBreak)
        {
            HideAll(hitbox);
            return;
        }

        foreach (var config in Icons)
        {
            EnsureSingleIcon(holder, hitbox, config);
        }
    }

    private static void EnsureSingleIcon(NHandCardHolder holder, Control hitbox, IconConfig config)
    {
        
        var model = holder.CardNode?.Model;
        if (model == null)
            return;
        
        var container = hitbox.GetNodeOrNull<Control>(config.ContainerName);
        
        bool shouldShow = config.ShouldShow?.Invoke(model) ?? true;

        if (!shouldShow)
        {
            if (container != null)
                container.Visible = false;

            return;
        }


        if (container == null)
        {
            var scene = GetScene(config.ScenePath);
            if (scene == null)
                return;

            container = scene.Instantiate<Control>();
            container.Name = config.ContainerName;

            // ✅ This is the ONLY interactive node
            container.MouseFilter = Control.MouseFilterEnum.Pass;

            hitbox.AddChild(container);
            hitbox.MoveChild(container, hitbox.GetChildCount() - 1);

            // Prevent children stealing hover
            SetChildControlsToIgnore(container);

            var capturedContainer = container;
            var capturedHolder = holder;
            var capturedConfig = config;

            container.MouseEntered += () =>
                OnHovered(capturedContainer, capturedHolder, capturedConfig);

            // container.MouseExited += () =>
            //     OnUnhovered(capturedContainer);
            
            
            container.MouseExited += () =>
                OnUnhovered(capturedHolder);

        }

        container.Visible = true;
        container.Position = config.Position;
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

            // ✅ Position it to the LEFT of the card (like hovercard)
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
public static class LimitCardPatch_Ready
{
    public static void Postfix(NHandCardHolder __instance)
    {
        Callable.From(() => LimitCardPatch.EnsureAndRefresh(__instance)).CallDeferred();
    }
}

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard))]
public static class LimitCardPatch_UpdateCard
{
    public static void Postfix(NHandCardHolder __instance)
    {
        LimitCardPatch.EnsureAndRefresh(__instance);
    }
}

#endregion
