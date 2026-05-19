using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Cards.Common;

public class MakoMight() : CloudCard(1, CardType.Power,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<PunisherModePower>(1m),
        new PowerVar<VigorPower>(4m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PunisherModePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (!base.Owner.Creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Apply<PunisherModePower>(choiceContext, base.Owner.Creature, base.DynamicVars["PunisherModePower"].BaseValue, base.Owner.Creature, this);
            if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            {
                SfxCmd.Play("res://Cloud/sounds/zenryokudeiku.wav");
                float duration = cloud.PlayAnimation(ownerCreature, "mode_shift").total;
                if (duration > 0f)
                    await Task.Delay((int)(duration * 0.9f * 1000f));
            }
        }
        await PowerCmd.Apply<VigorPower>(choiceContext, base.Owner.Creature, base.DynamicVars["VigorPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}