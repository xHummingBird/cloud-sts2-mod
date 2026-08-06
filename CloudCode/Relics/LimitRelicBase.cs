using BaseLib.Extensions;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Relics;

public abstract class LimitRelicBase : CloudRelic
{
    // Save/persistence values
    // Your LimitManager/SummonManager can read/write these instead of static dictionaries.
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<LimitBreakPower>(),
    ];

    // ---------- Override values ----------
    protected virtual int BaseTurnLimitGain => 3;
    protected virtual int FuryTurnLimitGain => 6;

    protected virtual int BaseAttackLimitGain => 3;
    
    protected virtual int FuryAttackLimitGain => 6;

    protected virtual int BaseTurnSummonGain => 3;
    protected virtual int SummonUpTurnSummonGain => 6;

    protected virtual int AttackSummonGain => 3;
    protected virtual int MagicSummonGain => 6;

    protected virtual int StartingTurnATBGain => 1;
    protected virtual int NonATBCardATBGain => 1;
    
    [SavedProperty]
    public int StoredLimit { get; set; }

    [SavedProperty]
    public int StoredSummon { get; set; }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;

        if (card.Owner != Owner)
            return;

        var player = Owner;

        bool isLimit = card is ILimitCard;
        bool isATB = card is IATBCard;
        bool isMagic = card is IMagicCard;
        bool isSummon = card is ISummonCard;

        if (!isATB)
            ATBManager.GainATBFromAttack(player, NonATBCardATBGain);

        // Limit and Summon cards should not generate normal resource gains.
        if (isLimit || isSummon)
            return;

        if (card.Type == CardType.Attack)
        {
            LimitManager.GainLimit(player, GetAttackLimitGain(card));
        }

        if (card.Type == CardType.Attack || isMagic)
        {
            SummonManager.GainSummon(player, isMagic ? MagicSummonGain : AttackSummonGain);
        }

        await Owner.Creature.CheckLimitReady(
            choiceContext,
            Owner.Creature,
            card
        );

        await Owner.Creature.CheckSummonReady(
            choiceContext,
            Owner.Creature,
            card
        );
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;

        if (card.Owner != Owner)
            return;
        
        if (base.Owner.Creature.HasPower<LimitBreakPower>())
            await CloudExtensions.AddLimitBreakToHand(base.Owner);

        if (base.Owner.Creature.HasPower<SummonPower>() && !base.Owner.Creature.HasPower<MegaflarePower>())
            await CloudExtensions.AddSummonToHand(base.Owner);
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Creature.Side)
            return;

        if (combatState.RoundNumber <= 1)
        {
            ATBManager.Reset(Owner.Creature.Player);
        }

        if (ATBManager.GetATB(Owner.Creature.Player) == 0)
        {
            Flash();
            ATBManager.GainATBDirect(Owner.Creature.Player, StartingTurnATBGain);
        }

        SfxCmd.Play("event:/sfx/ui/relic_activate_general");

        LimitManager.GainLimit(Owner, GetTurnLimitGain());
        SummonManager.GainSummon(Owner, GetTurnSummonGain());
        
        await Owner.Creature.CheckLimitReady(
            null,
            Owner.Creature,
            null
        );
        
        await Owner.Creature.CheckSummonReady(
            null,
            Owner.Creature,
            null
        );
    }
    
    public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side)
            return;
        
        if (base.Owner.Creature.HasPower<LimitBreakPower>())
            await CloudExtensions.AddLimitBreakToHand(base.Owner);
        
        if (base.Owner.Creature.HasPower<SummonPower>() && !base.Owner.Creature.HasPower<MegaflarePower>())
            await CloudExtensions.AddSummonToHand(base.Owner);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature)
            return;

        if (dealer == Owner.Creature)
            return;

        if (!props.IsPoweredAttack())
            return;

        int gain = 0;

        if (result.BlockedDamage > 0 && result.UnblockedDamage == 0)
        {
            gain += 3;
        }

        if (result.UnblockedDamage > 0)
        {
            gain += result.UnblockedDamage;
        }

        gain = ModifyDamageTakenLimitGain(gain);

        if (gain <= 0)
            return;

        LimitManager.GainLimit(Owner, gain);

        // await Owner.Creature.CheckLimitReady(
        //     choiceContext,
        //     Owner.Creature,
        //     null
        // );
    }

    protected virtual int GetAttackLimitGain(CardModel card)
    {
        if (Owner.HasPower<FuryPower>())
            return FuryAttackLimitGain;
        
        return BaseAttackLimitGain;
    }

    protected virtual int GetTurnLimitGain()
    {
        if (Owner.HasPower<FuryPower>())
            return FuryTurnLimitGain;

        return BaseTurnLimitGain;
    }

    protected virtual int GetTurnSummonGain()
    {
        if (Owner.HasPower<SummonUpPower>())
            return SummonUpTurnSummonGain;

        return BaseTurnSummonGain;
    }

    protected virtual int ModifyDamageTakenLimitGain(int gain)
    {
        if (Owner.HasPower<SedatePower>())
            return gain / 2;

        return gain;
    }
}