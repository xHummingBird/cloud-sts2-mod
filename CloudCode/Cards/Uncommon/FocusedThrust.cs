using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
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
        new DamageVar(8m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1),
        new("percentHPDamage", 5)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay? play)
    {
        decimal finalDamage;
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            await cloud.DashTo(ownerCreature, play.Target, distance: 550f);
            float duration = cloud.PlayAnimation(ownerCreature, "focused_thrust").total;
            SfxCmd.Play("res://Cloud/sounds/sokoda.wav");
            if (duration > 0f)
            {
                await Task.Delay((int)(0.1f * 1000f));
                await Task.Delay((int)(0.1f * 1000f));
                CloudExtensions.CombatHelpers.FakeHit(play.Target);
                await Task.Delay((int)(0.45f * 1000f));
                SfxCmd.Play("res://Cloud/sounds/heavy_attack (2).wav");
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                await Task.Delay((int)(0.12f * 1000f));
                finalDamage = (play.Target.CurrentHp * (DynamicVars["percentHPDamage"].BaseValue)/100) + DynamicVars.Damage.BaseValue;
                await DamageCmd.Attack(finalDamage).FromCard(this, play).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash", "res://Cloud/sfx/cloud_hit.wav")
                    .Execute(choiceContext);
                await Task.Delay((int)(0.4f * 1000f));
                await cloud.Retreat(ownerCreature);
            }
        }
        else
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play)
                .Execute(choiceContext);
            finalDamage = (play.Target.CurrentHp * (DynamicVars["percentHPDamage"].BaseValue)/100) + DynamicVars.Damage.BaseValue;
            await DamageCmd.Attack(finalDamage).FromCard(this, play).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        if (play.Target !=null)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["percentHPDamage"].UpgradeValueBy(3);
    }
}