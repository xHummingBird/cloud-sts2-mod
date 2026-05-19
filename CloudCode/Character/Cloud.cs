using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Cards.Basic;
using Cloud.CloudCode.Cards.Common;
using Cloud.CloudCode.Cards.Rare;
using Cloud.CloudCode.Cards.Uncommon;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using Cloud.CloudCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using Thunder = Cloud.CloudCode.Cards.Basic.Thunder;

namespace Cloud.CloudCode.Character;

public class Cloud : PlaceholderCharacterModel
{
    public const string CharacterId = "Cloud";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 80;
    
    private enum CloudMode
    {
        Operator,
        Punisher
    }
    
    
    private CloudMode GetMode(Creature creature)
    {
        if (creature == null)
            return CloudMode.Operator;

        // TODO: replace with your actual buff check
        if (creature.HasPower<PunisherModePower>())
            return CloudMode.Punisher;

        return CloudMode.Operator;
    }
    
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeCloud>(),
        ModelDb.Card<StrikeCloud>(),
        // ModelDb.Card<Braver>(),
        ModelDb.Card<StrikeCloud>(),
        ModelDb.Card<StrikeCloud>(),
        // ModelDb.Card<Fire>(),
        // ModelDb.Card<Blizzard>(),
        // ModelDb.Card<Thunder>(),
        ModelDb.Card<DefendCloud>(),
        ModelDb.Card<DefendCloud>(),
        ModelDb.Card<DefendCloud>(),
        ModelDb.Card<DefendCloud>(),
        ModelDb.Card<ModeShift>(),
        
        // for testing
        ModelDb.Card<AerialDrive>(),
        ModelDb.Card<CrossSlash>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BusterSword>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<CloudCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<CloudRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<CloudPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    
    private const string CustomVisualScenePath = "res://Cloud/scenes/cloud.tscn";
    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();

// =========================================================
    //  VISUALS: Load your sprite-based .tscn (Visuals + AnimationPlayer + Bounds)
    // =========================================================
    public override NCreatureVisuals? CreateCustomVisuals()
            {
                // This converts your .tscn into an NCreatureVisuals-compatible instance.
                // Your scene currently has: Visuals (Node2D), AnimationPlayer, Bounds (Control).
                return NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualScenePath);
            }

    // If you're using sprite-based visuals, you typically do NOT want a MegaSpine animator.
    public override CreatureAnimator? GenerateAnimator(MegaSprite controller) => null;

    // =========================================================
    //  BASIC ANIMATION PLAYER BRIDGE
    // =========================================================
   
    
    private string GetIdleAnimation(Creature creature)
    {
        return GetMode(creature) == CloudMode.Punisher
            ? "idle_punisher"
            : "idle_operator";
    }

    
    public (float total, float[] impacts) PlayAnimation(Creature creature, string trigger)
    {
        
        if (creature == null || string.IsNullOrEmpty(trigger))
            return (0f, Array.Empty<float>());

        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node?.Visuals == null)
            return (0f, Array.Empty<float>());

        var animPlayer = node.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (animPlayer == null)
            return (0f, Array.Empty<float>());
        var mode = GetMode(creature);
        
        string godotTrigger = (trigger.ToLowerInvariant(), mode) switch
        {
            
            ("attack", CloudMode.Punisher) => "attack_punisher",
            ("attack", _) => "attack_operator",
            
            ("idle", _) or ("idle_loop", _) => GetIdleAnimation(creature),
            
            ("dead", _) => "dead",
            ("die", _) => "dead",
            
            ("cast", CloudMode.Punisher) => "cast_punisher",
            ("cast", _) => "cast_operator",

            _ => trigger
        };

        if (!animPlayer.HasAnimation(godotTrigger))
            return (0f, Array.Empty<float>());

        var anim = animPlayer.GetAnimation(godotTrigger);
        float totalLength = (float)anim.Length;

        animPlayer.Play(godotTrigger);
        
        
        if (godotTrigger != "die")
        {
            string nextIdle = GetIdleAnimation(creature);
            animPlayer.Queue(nextIdle);
        }
        
        return (totalLength, Array.Empty<float>());
    } // ✅ CLOSE PlayAnimation HERE
    
    
    public void RefreshIdle(Creature creature)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        var animPlayer = node?.Visuals?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

        if (animPlayer == null)
            return;

        string idle = GetIdleAnimation(creature);

        if (animPlayer.CurrentAnimation != idle)
            animPlayer.Play(idle);
    }
    
    // =========================================================
    //  HARMONY PATCHES: minimal trigger routing & death duration
    // =========================================================

    [HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
    public static class NCreatureSetTriggerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NCreature __instance, string trigger)
        {
            // This ensures the engine's triggers automatically drive your AnimationPlayer.
            if (__instance.Entity?.Player?.Character is Cloud character)
            {
                character.PlayAnimation(__instance.Entity, trigger);
                return false; // skip default skeletal animation path
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
    public static class StartDeathAnimPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCreature __instance, ref float __result)
        {
            // Make the game wait for your Godot "die" animation length.
            if (__instance.Entity?.Player?.Character is Cloud character)
            {
                AudioHelper.PlayRandomGameover();
                character.PlayAnimation(__instance.Entity, "die");

                var animPlayer = __instance.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                __result = animPlayer?.GetAnimation("die")?.Length ?? 1.5f;
            }
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterPowerAmountChanged))]
    public static class CloudModeChangePatch
    {
        public static void Postfix(CombatState combatState, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            var target = power.Owner;
            if (target?.Player?.Character is Cloud character)
            {
                character.RefreshIdle(target);
            }
        }
    }
    
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    public static class CloudVictoryAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(IRunState runState, CombatState? combatState)
        {
            var creature = combatState?.Creatures?.FirstOrDefault(c => c.IsPlayer);
            
            if (creature?.Player?.Character is not Cloud)
                return;
            var node = NCombatRoom.Instance?.GetCreatureNode(creature);
            var animPlayer = node?.Visuals?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

            
            if (animPlayer == null)
                return;
            
            // Play pre-animation
            
            if (animPlayer.HasAnimation("victory_before"))
            {
                AudioHelper.PlayRandomVictory();
                animPlayer.Play("victory_before");

                // Then loop victory
                if (animPlayer.HasAnimation("victory"))
                    animPlayer.Queue("victory");
            }
            else
            {
                // fallback if missing
                animPlayer.Play("victory");
            }

        }
    }
    
   [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
    public static class CloudDamageAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Creature target, DamageResult result, ValueProp props, Creature? dealer)
        {
            // Only Cloud
            if (target.Player?.Character is not Cloud character)
                return;

            // Only enemy-caused damage (avoid self-damage, environment, etc.)
            if (dealer == null || dealer.Side != CombatSide.Enemy)
                return;

            // Respect engine flags
            if (props.HasFlag(ValueProp.SkipHurtAnim) || props.HasFlag(ValueProp.Unpowered))
                return;

            if (result.WasFullyBlocked && result.BlockedDamage > 0)
            {
                    character.PlayAnimation(target, "block"); 
            }
            
            // Only when HP damage actually happened
            else if (result.UnblockedDamage > 0 && !target.IsDead)
            {
                character.PlayAnimation(target, "block");
                if (target.CurrentHp < 20)
                {
                    AudioHelper.PlayRandomDamagedCritical();
                }
                else if (result.UnblockedDamage < 10)
                {
                    AudioHelper.PlayRandomDamaged();
                }
                else AudioHelper.PlayRandomDamagedHigh();// maps to "hurt" in your PlayAnimation mapping
            }
        }
    }

}