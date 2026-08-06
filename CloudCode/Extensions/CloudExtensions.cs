using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using Cloud.CloudCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Extensions;

public static class CloudExtensions
{
    public static bool IsPunisher(this Creature creature)
        => creature.HasPower<PunisherModePower>();

    public static bool IsOperator(this Creature creature)
        => !creature.IsPunisher();
    
    public static bool IsPrime(this Creature creature)
        => creature.HasPower<PrimeModePower>();

    public static async Task TogglePunisher(this Creature creature,
        decimal amount,
        Creature source,
        CardModel card)
    {
        if (creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Remove<PunisherModePower>(creature);
        }
        else
        {
            await PowerCmd.Apply<PunisherModePower>(new ThrowingPlayerChoiceContext(), creature, 1, source, card);
        }
    }
    
    public static async Task EnterPunisher(this Creature creature,
        decimal amount,
        Creature source,
        CardModel card)
    {
        if (!creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Apply<PunisherModePower>(new ThrowingPlayerChoiceContext(),creature, 1, source, card);
        }
    }

    public static async Task ExitPunisher(this Creature? creature)
    {
        if (creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Remove<PunisherModePower>(creature);
        }
    }
    
    public static async Task CheckLimitReady(
        this Creature creature,
        PlayerChoiceContext? context,
        Creature source,
        CardModel? card)
    {
        var player = creature.Player;
        if (player == null)
            return;

        if (LimitManager.IsFull(player) &&
            !creature.HasPower<LimitBreakPower>())
        {
            await PowerCmd.Apply<LimitBreakPower>(
                context,
                creature,
                1,
                creature,
                null
            );
        }
    }
    
    public static async Task CheckSummonReady(
        this Creature creature,
        PlayerChoiceContext? context,
        Creature source,
        CardModel? card)
    {
        var player = creature.Player;
        if (player == null)
            return;

        if (SummonManager.IsFull(player) &&
            !creature.HasPower<SummonPower>())
        {
            await PowerCmd.Apply<SummonPower>(
                context,
                creature,
                1,
                creature,
                null
            );
        }
    }
    
    public static class CombatHelpers
    {
        private const string DefaultSfx = "res://Cloud/sfx/sword_swing_heavy.wav";
        private const string DefaultVfx = "vfx/vfx_attack_slash";

        public static async Task FakeHit(
            Creature target,
            string? sfxPath = null)
        {
            if (target == null) return;

            SfxCmd.Play(sfxPath ?? DefaultSfx);
            VfxCmd.PlayOnCreatureCenter(target, DefaultVfx);
            SfxCmd.Play(target.Monster.TakeDamageSfx);
            SfxCmd.Play("res://Cloud/sfx/cloud_hit.wav");
            await CreatureCmd.TriggerAnim(target, "Hit", 0f);
            
            if (target.Monster?.HasHurtSfx == true)
            {
                SfxCmd.Play(target.Monster.HurtSfx);
            }
        }
    }
    
    public static async Task AddLimitBreakToHand(Player player)
    {
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        var playerState = player.PlayerCombatState;

        // Already in hand, do nothing.
        if (playerState.AllCards
            .OfType<LimitBreak>()
            .Any(c => c.Pile?.Type == PileType.Hand))
        {
            return;
        }

        // Find an existing Limit Break anywhere (draw/discard/exhaust/etc.)
        var limitBreak = playerState.AllCards
            .OfType<LimitBreak>()
            .FirstOrDefault();

        if (limitBreak != null)
        {
            ChampionBelt? championBelt = player.GetRelic<ChampionBelt>();
            if (championBelt != null)
            {
                CardCmd.Upgrade(limitBreak);
            }
            await CardPileCmd.Add(limitBreak, PileType.Hand);
        }
        else
        {
            limitBreak = player.Creature.CombatState
                .CreateCard<LimitBreak>(player);
            
            ChampionBelt? championBelt = player.GetRelic<ChampionBelt>();
            if (championBelt != null)
            {
                CardCmd.Upgrade(limitBreak);
            }

            await CardPileCmd.AddGeneratedCardToCombat(
                limitBreak,
                PileType.Hand,
                player);
        }
    }
    
    public static async Task AddSummonToHand(Player player)
    {
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        if (player.Creature.HasPower<MegaflarePower>())
            return;

        var playerState = player.PlayerCombatState;

        // Already in hand, do nothing.
        if (playerState.AllCards
            .OfType<SummonCard>()
            .Any(c => c.Pile?.Type == PileType.Hand))
        {
            return;
        }

        // Find an existing Limit Break anywhere (draw/discard/exhaust/etc.)
        var summonCard = playerState.AllCards
            .OfType<SummonCard>()
            .FirstOrDefault();

        if (summonCard != null)
        {
            if (player.Creature.HasPower<SummonUpPower>())
            {
                CardCmd.Upgrade(summonCard);
            }
            await CardPileCmd.Add(summonCard, PileType.Hand);
        }
        else
        {
            summonCard = player.Creature.CombatState
                .CreateCard<SummonCard>(player);
            
            if (player.Creature.HasPower<SummonUpPower>())
            {
                CardCmd.Upgrade(summonCard);
            }

            await CardPileCmd.AddGeneratedCardToCombat(
                summonCard,
                PileType.Hand,
                player);
        }
    }
}
