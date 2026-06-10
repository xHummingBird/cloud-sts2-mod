using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Cloud.CloudCode.Cards.Rare;

public class WMagic() : CloudCard(3, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<WMagicPower>(1m),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WMagicPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WMagicPower>(choiceContext, base.Owner.Creature, base.DynamicVars["WMagicPower"].BaseValue, base.Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}