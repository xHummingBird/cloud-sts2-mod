using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Cards.Uncommon;

public class SoldiersPride() : CloudCard(2, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
protected override IEnumerable<DynamicVar> CanonicalVars => 
[
    new PowerVar<SoldiersPridePower>(5m),
    ];
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    HoverTipFactory.FromPower<PunisherModePower>(),
    HoverTipFactory.FromPower<VigorPower>()
];

protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    await PowerCmd.Apply<SoldiersPridePower>(choiceContext, base.Owner.Creature, base.DynamicVars["SoldiersPridePower"].BaseValue, base.Owner.Creature, this);
}
protected override void OnUpgrade()
{
    DynamicVars["SoldiersPridePower"].UpgradeValueBy(2);
}
}