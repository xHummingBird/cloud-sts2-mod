using BaseLib.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Cloud.CloudCode.Cards.Uncommon;

public class ForgottenIdentity() : CloudCard(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
        protected override IEnumerable<DynamicVar> CanonicalVars => 
        [
            new PowerVar<SedatePower>(1m),
        ];
    
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<SedatePower>()
        ];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<SedatePower>(choiceContext, base.Owner.Creature, base.DynamicVars["SedatePower"].BaseValue, base.Owner.Creature, this);
            if (base.Owner.HasPower<FuryPower>())
                await PowerCmd.Remove<FuryPower>(base.Owner.Creature);
        }
        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
}