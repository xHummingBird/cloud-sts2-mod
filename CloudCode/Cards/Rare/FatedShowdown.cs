using BaseLib.Extensions;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Cards.Rare;

public class FatedShowdown() : CloudCard(3, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal =>
        LimitManager.GetLimit(base.Owner) >= 50;
    
    private IEnumerable<CardModel> GetLimitBreakCards()
    {
        var pile = PileType.Hand.GetPile(base.Owner);
        return pile.Cards.OfType<LimitBreak>();
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<StrengthPower>(3m),
        new PowerVar<PlatingPower>(3m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PunisherModePower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<LimitBreakPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)

    {
        decimal amountGain = DynamicVars.Strength.BaseValue;
        
        if (LimitManager.GetLimit(base.Owner) >= 50)
        {
            LimitManager.SpendLimit(base.Owner,50);
            amountGain *= 2;
        }

        if (base.Owner.HasPower<LimitBreakPower>())
        {
            foreach (var card in GetLimitBreakCards().ToList())
            {
                await CardCmd.Exhaust(choiceContext, card);
            }
            await PowerCmd.Remove<LimitBreakPower>(base.Owner.Creature);
        }
        
        var ownerCreature = Owner?.Creature;
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            AudioHelper.PlayRandomDefend();
        
        if (ownerCreature.HasPower<PunisherModePower>())
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, amountGain, base.Owner.Creature, this);
        else await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, amountGain, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1m);
        DynamicVars["PlatingPower"].UpgradeValueBy(1m);
    }
}