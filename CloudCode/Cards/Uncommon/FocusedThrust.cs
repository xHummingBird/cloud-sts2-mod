using BaseLib.Utils;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class FocusedThrust() : CloudCard(1, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy), IATBCard
{
    public int ATBCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(2),
        new PowerVar<VulnerablePower>(1),
        new("percentHPDamage", 5)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        decimal bonusDamage = play.Target.CurrentHp * (DynamicVars["percentHPDamage"].BaseValue)/100;
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            await cloud.DashTo(ownerCreature, play.Target, distance: 550f);
            float duration = cloud.PlayAnimation(ownerCreature, "focused_thrust").total;
            SfxCmd.Play("res://Cloud/sounds/sokoda.wav");
            if (duration > 0f)
            {
                await Task.Delay((int)(0.1f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                await Task.Delay((int)(0.1f * 1000f));
                DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                await Task.Delay((int)(0.45f * 1000f));
                SfxCmd.Play("res://Cloud/sounds/heavy_attack (2).wav");
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                await Task.Delay((int)(0.12f * 1000f));
                DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + bonusDamage).FromCard(this).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                await Task.Delay((int)(0.4f * 1000f));
                await cloud.Retreat(ownerCreature);
            }
        }
        else
        {
            DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + bonusDamage).FromCard(this).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        if (play.Target !=null)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["percentHPDamage"].UpgradeValueBy(2);
    }
}