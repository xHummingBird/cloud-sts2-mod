using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Cards.Common;

public class LargeSwordMastery() : CloudCard(1, CardType.Power,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<StrengthPower>(2m),
        new PowerVar<DexterityPower>(2m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PunisherModePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            AudioHelper.PlayRandomDefend();
        if (base.Owner.Creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, base.DynamicVars.Strength.BaseValue, base.Owner.Creature, this);
        }
        else await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner.Creature, base.DynamicVars.Dexterity.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1m);
        DynamicVars.Dexterity.UpgradeValueBy(1m);
    }
}