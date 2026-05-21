using BaseLib.Utils;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class TripleSlash() : CloudCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(4m),
        new RepeatVar(3),
        new ExtraDamageVar(2m),
        new CardsVar(1),
        new CalculatedDamageVar(ValueProp.Move)
        .WithMultiplier((CardModel _, Creature? dealer) =>
        dealer?.HasPower<PunisherModePower>() == true ? 0 : 1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/shizume.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "triple_slash").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.133f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                await Task.Delay((int)(0.333f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                await Task.Delay((int)(0.6f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                SfxCmd.Play("res://Cloud/sounds/heavy_attack (2).wav");
                await CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                await Task.Delay((int)(0.4f * 1000f));
            }
        }
        else
        {
            await CommonActions.CardAttack(this, play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        if (base.Owner.Creature.HasPower<PunisherModePower>())
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(2);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}