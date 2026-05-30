using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Cloud.CloudCode.Cards.Uncommon;

public class CombatMomentum() : CloudCard(2, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<CombatMomentumPower>(1m),
        new CardsVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PunisherModePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
            await PowerCmd.Apply<CombatMomentumPower>(choiceContext, base.Owner.Creature, base.DynamicVars["CombatMomentumPower"].BaseValue, base.Owner.Creature, this);
        }
    protected override void OnUpgrade()
    {
        DynamicVars["CombatMomentumPower"].UpgradeValueBy(1);
    }
}
    
