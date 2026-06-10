using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Cloud.CloudCode.Cards.Rare;

public class AdrenalineSurge() : CloudCard(2, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<LimitBreakPower>(30m)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FuryPower>(),
        HoverTipFactory.FromPower<LimitBreakPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int finalAmount = DynamicVars["LimitBreakPower"].IntValue;
        if (Owner.Creature.HasPower<FuryPower>())
            finalAmount += 20;
        
        LimitManager.GainLimit(Owner, finalAmount);
        await Owner.Creature.CheckLimitReady(
            choiceContext,
            Owner.Creature,
            null);
        
    }
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}