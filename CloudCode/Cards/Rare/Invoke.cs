using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class Invoke() : CloudCard(2, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SummonPower>(15),
        new PowerVar<SummonUpPower>(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SummonPower>(),
        HoverTipFactory.FromPower<SummonUpPower>()
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SummonManager.GainSummon(Owner, DynamicVars["SummonPower"].IntValue);
        await Owner.Creature.CheckSummonReady(
            choiceContext,
            Owner.Creature,
            null
        );
        await PowerCmd.Apply<SummonUpPower>(choiceContext, base.Owner.Creature, DynamicVars["SummonUpPower"].BaseValue, base.Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        DynamicVars["SummonPower"].UpgradeValueBy(5m);
    }
    
}