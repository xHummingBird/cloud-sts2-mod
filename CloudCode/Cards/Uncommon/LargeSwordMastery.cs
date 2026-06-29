using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Cards.Uncommon;

public class LargeSwordMastery() : CloudCard(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<StrengthPower>(2m),
        new PowerVar<DexterityPower>(2m),
        new PowerVar<GreatswordMasteryPower>(2m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<GreatswordMasteryPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            AudioHelper.PlayRandomDefend();
        await PowerCmd.Apply<GreatswordMasteryPower>(choiceContext, base.Owner.Creature, base.DynamicVars["GreatswordMasteryPower"].BaseValue, base.Owner.Creature, this);
       
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1m);
        DynamicVars.Dexterity.UpgradeValueBy(1m);
        DynamicVars["GreatswordMasteryPower"].UpgradeValueBy(1m);
    }
}