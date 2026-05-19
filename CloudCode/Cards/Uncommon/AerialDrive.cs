using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class AerialDrive() : CloudCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move),
        new RepeatVar(2),
        new PowerVar<LimitBreakPower>(10)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/futobe.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "aerial_drive").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.2f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                
                await Task.Delay((int)(0.49f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                await CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
            }
        }
        else await CommonActions.CardAttack(this, play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LimitBreakPower"].UpgradeValueBy(5);
    }
}
