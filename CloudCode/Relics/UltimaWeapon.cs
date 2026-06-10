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
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Relics;

public class UltimaWeapon() : CloudRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<LimitBreakPower>(),
    ];
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        
        var card = cardPlay.Card;

        if (card.Owner != Owner) return;

        var player = Owner;
        
        bool isLimit  = card is ILimitCard;
        bool isATB    = card is IATBCard;
        bool isMagic  = card is IMagicCard;
        bool isSummon = card is ISummonCard;

        if (isLimit || isSummon) return;
        if (card.Type == CardType.Attack && !isATB)
            ATBManager.GainATBFromAttack(player, 1);
        if (card.Type == CardType.Attack)
        {
            if (card.Owner.HasPower<FuryPower>())
                LimitManager.GainLimit(player, 6);
            else if (card.Owner.HasPower<SedatePower>())
                LimitManager.GainLimit(player, 1);
            else LimitManager.GainLimit(player, 3);
        }

        if (card.Type == CardType.Attack || isMagic)
        {
            SummonManager.GainSummon(player, isMagic ? 5 : 1);
        }
        
        await Owner.Creature.CheckLimitReady(
            choiceContext,
            Owner.Creature,
            cardPlay.Card
        );

        return;
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        
        if (side != base.Owner.Creature.Side)
            return;
        if (combatState.RoundNumber <= 1)
        {
            ATBManager.Reset(Owner.Creature.Player);
        }
        if (ATBManager.GetATB(Owner.Creature.Player) == 0)
            ATBManager.GainATBDirect(Owner.Creature.Player, 1);
        // ATBManager.ResetGainThisTurn(Owner);
        SfxCmd.Play("event:/sfx/ui/relic_activate_general");
        if (base.Owner.HasPower<SedatePower>())
            LimitManager.GainLimit(Owner, 3);
        else if (base.Owner.HasPower<FuryPower>())
            LimitManager.GainLimit(Owner, 8);
        else LimitManager.GainLimit(Owner, 5);
        await Owner.Creature.CheckLimitReady(
            null,
            Owner.Creature,
            null
        );
    }
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner.Creature)
            return;

        if (dealer == base.Owner.Creature)
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
        LimitManager.GainLimit(Owner, gain);
        await Owner.Creature.CheckLimitReady(
            choiceContext,
            Owner.Creature,
            null
        );
    }
}