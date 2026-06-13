using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Cloud.CloudCode.Extensions;

public static class CinematicAttack
{
    public static bool IsHidden { get; private set; } = false;

    private static readonly FieldInfo _isDebugHiddenField =
        typeof(NCombatUi).GetField("_isDebugHidden", BindingFlags.Static | BindingFlags.NonPublic);

    private static readonly FieldInfo _isDebugHidingHandField =
        typeof(NCombatUi).GetField("_isDebugHidingHand", BindingFlags.Static | BindingFlags.NonPublic);

    private static readonly PropertyInfo _isDebugHidingPlayContainerProp =
        typeof(NCombatUi).GetProperty("IsDebugHidingPlayContainer",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

    private static readonly PropertyInfo _isDebugHidingIntentProp =
        typeof(NCombatUi).GetProperty("IsDebugHidingIntent",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

    private static readonly PropertyInfo _isDebugHidingHpBarProp =
        typeof(NCombatUi).GetProperty("IsDebugHidingHpBar",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    
    private static readonly FieldInfo _debugToggleIntentField =
        typeof(NCombatUi).GetField("DebugToggleIntent", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo _debugToggleHpBarField =
        typeof(NCombatUi).GetField("DebugToggleHpBar", BindingFlags.Instance | BindingFlags.NonPublic);
    
    private static readonly FieldInfo _topBarDebugHiddenField =
        typeof(NTopBar).GetField("_isDebugHidden", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo _relicsDebugHiddenField =
        typeof(NRelicInventory).GetField("_isDebugHidden", BindingFlags.Instance | BindingFlags.NonPublic);
    
    public static void Start(ulong triggeringPlayerNetId)
    {
        if (!IsLocalPlayer(triggeringPlayerNetId))
            return;

        if (IsHidden)
            return;

        SetCombatUiHidden(true);
        IsHidden = true;
    }

    public static void End(ulong triggeringPlayerNetId)
    {
        if (!IsLocalPlayer(triggeringPlayerNetId))
            return;

        if (!IsHidden)
            return;

        SetCombatUiHidden(false);
        IsHidden = false;
    }

    private static bool IsLocalPlayer(ulong triggeringPlayerNetId)
    {
        return RunManager.Instance != null
               && RunManager.Instance.NetService != null
               && triggeringPlayerNetId == RunManager.Instance.NetService.NetId;
    }

    private static void SetCombatUiHidden(bool hidden)
    {
        var combatUi = NCombatRoom.Instance?.Ui;
        if (combatUi == null)
            return;

        Color targetColor = hidden ? Colors.Transparent : Colors.White;

        _isDebugHiddenField?.SetValue(null, hidden);
        _isDebugHidingHandField?.SetValue(null, hidden);
        _isDebugHidingPlayContainerProp?.SetValue(null, hidden);
        _isDebugHidingIntentProp?.SetValue(null, hidden);
        _isDebugHidingHpBarProp?.SetValue(null, hidden);
        
        (_debugToggleIntentField?.GetValue(combatUi) as Action)?.Invoke();
        (_debugToggleHpBarField?.GetValue(combatUi) as Action)?.Invoke();
        
        foreach (var child in combatUi.GetChildren().OfType<Control>())
        {
            child.Modulate = targetColor;
        }
        
        var globalUi = NRun.Instance?.GlobalUi;
        if (globalUi == null)
            return;

        bool hide = hidden;

        if (globalUi.TopBar != null)
        {
            _topBarDebugHiddenField?.SetValue(globalUi.TopBar, hide);
            globalUi.TopBar.Modulate = targetColor;
        }

        if (globalUi.RelicInventory != null)
        {
            _relicsDebugHiddenField?.SetValue(globalUi.RelicInventory, hide);
            globalUi.RelicInventory.Modulate = targetColor;
        }

    }
}


//     private static void SetCombatUiHidden(bool hidden)
    //     {
    //         var combatUi = NCombatRoom.Instance?.Ui;
    //         if (combatUi == null)
    //             return;
    //         
    //         Color targetColor = hidden ? Colors.Transparent : Colors.White;
    //         
    //         _isDebugHiddenField?.SetValue(null, hidden);
    //         _isDebugHidingHandField?.SetValue(null, hidden);
    //         _isDebugHidingPlayContainerProp?.SetValue(null, hidden);
    //         _isDebugHidingIntentProp?.SetValue(null, hidden);
    //         _isDebugHidingHpBarProp?.SetValue(null, hidden);
    //
    //         // foreach (var child in combatUi.GetChildren().OfType<Control>())
    //         // {
    //         //     child.Visible = !hidden; // ✅ CLEAN TOGGLE
    //         // }
    //         
    //         foreach (var child in combatUi.GetChildren().OfType<Control>())
    //         {
    //             child.Modulate = targetColor;
    //         }
    //     }
    //
    // }
    //
    // [HarmonyPatch(typeof(NCombatRoom), "AddCreature")]
    // public static class FixSpawnedUiPatch
    // {
    //     [HarmonyPostfix]
    //     public static void Postfix(NCombatRoom __instance, Creature creature)
    //     {
    //         if (!CinematicAttack.IsHidden)
    //             return;
    //
    //         var node = __instance.GetCreatureNode(creature);
    //         if (node == null) return;
    //
    //         // Force UI visible immediately for new spawns
    //         foreach (var child in node.GetChildren())
    //         {
    //             if (child is CanvasItem canvas)
    //             {
    //                 canvas.Visible = true;
    //                 canvas.Modulate = Colors.White;
    //             }
    //         }
    //
    //         if (node.Visuals != null)
    //         {
    //             foreach (var child in node.Visuals.GetChildren())
    //             {
    //                 if (child is CanvasItem canvas)
    //                 {
    //                     canvas.Visible = true;
    //                     canvas.Modulate = Colors.White;
    //                 }
    //             }
    //         }
    //     }
    // }


