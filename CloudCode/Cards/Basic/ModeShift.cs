using Cloud.CloudCode.Cards;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Cloud.CloudCode.Cards.Basic;

public class ModeShift() : CloudCard(0, CardType.Skill,
    CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [
            new PowerVar<PunisherModePower>(1m),
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
            if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            {
                SfxCmd.Play("res://Cloud/sounds/zenryokudeiku.wav");
                float duration = cloud.PlayAnimation(ownerCreature, "mode_shift").total;
                if (duration > 0f)
                    await Task.Delay((int)(duration * 0.9f * 1000f));
                cloud.PlayAnimation(ownerCreature, "idle_punisher");
            }
            PowerCmd.Apply<PunisherModePower>(choiceContext, base.Owner.Creature, base.DynamicVars["PunisherModePower"].BaseValue, base.Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Remove<PunisherModePower>(base.Owner.Creature);
            if (Owner?.Character is Character.Cloud cloud)
            {
                cloud.RefreshIdle(ownerCreature);
            }
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}