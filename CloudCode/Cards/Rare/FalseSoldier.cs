using BaseLib.Extensions;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class FalseSoldier() : CloudCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override bool ShouldGlowGoldInternal =>
        LimitManager.GetLimit(base.Owner) >= 50;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(20m, ValueProp.Move),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    private IEnumerable<CardModel> GetLimitBreakCards()
    {
        var pile = PileType.Hand.GetPile(base.Owner);
        return pile.Cards.OfType<LimitBreak>();
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        if (LimitManager.GetLimit(base.Owner) >= 50)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        }
        LimitManager.SetLimit(base.Owner, 0);
        if (base.Owner.HasPower<LimitBreakPower>())
        {
            foreach (var card in GetLimitBreakCards().ToList())
            {
                await CardCmd.Exhaust(choiceContext, card);
            }
            await PowerCmd.Remove<LimitBreakPower>(base.Owner.Creature);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(5m);
    }
}