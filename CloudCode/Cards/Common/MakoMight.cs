using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Common;

public class MakoMight() : CloudCard(1, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<PunisherModePower>(1m),
        new PowerVar<VigorPower>(5m),
        new BlockVar(5, ValueProp.Move)
        
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PunisherModePower>(),
        HoverTipFactory.FromPower<VigorPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (!ownerCreature.IsPunisher())
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, base.Owner.Creature,
                base.DynamicVars["VigorPower"].BaseValue, base.Owner.Creature, this);
        }
        else
        {
            AudioHelper.PlayRandomDefend();
            await CommonActions.CardBlock(this, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}