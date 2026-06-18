using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class CrossSlashKai() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), ILimitCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(30, ValueProp.Move),
        new PowerVar<CrossSlashPower>(80),
        new RepeatVar(3)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sfx/energy_charge.wav");
            SfxCmd.Play("res://Cloud/sounds/limit_break.wav");
            SfxCmd.Play("res://Cloud/sfx/limit_break_thunder.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "limit_break_1").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(duration * 1000f));
                CinematicAttack.Start(RunManager.Instance.NetService.NetId);
                
                await cloud.DashTo(ownerCreature, play.Target, distance: 550f);
                
                cloud.PlayAnimation(ownerCreature, "cross_slash");
                await Task.Delay((int)(0.85f * 1000f));
                CloudExtensions.CombatHelpers.FakeHit(play.Target);
                cloud.DoScreenShake(ShakeStrength.Weak, ShakeDuration.Normal);
                
                await Task.Delay((int)(0.60f * 1000f));
                SfxCmd.Play("res://Cloud/sounds/heavy_attack (3).wav");
                CloudExtensions.CombatHelpers.FakeHit(play.Target);
                cloud.DoScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
                
                await Task.Delay((int)(0.80f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
                SfxCmd.Play("res://Cloud/sounds/ungawarukatana.wav");
                CommonActions.CardAttack(this, play.Target)
                    .WithHitFx("vfx/vfx_attack_slash", "res://Cloud/sfx/cloud_hit.wav")
                    .Execute(choiceContext);
                cloud.DoScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
                
                await Task.Delay((int)(0.20f * 1000f));
                if (play.Target != null)
                {
                    PowerCmd.Apply<CrossSlashPower>(choiceContext, play.Target,
                        base.DynamicVars["CrossSlashPower"].BaseValue,
                        base.Owner.Creature, this);
                }

                await Task.Delay((int)(0.30f * 1000f));
                // cam?.EndCinematic();
                CinematicAttack.End(RunManager.Instance.NetService.NetId);
                await cloud.Retreat(ownerCreature);
            }
        }
        else
        {
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<CrossSlashPower>(choiceContext, play.Target,
                base.DynamicVars["CrossSlashPower"].BaseValue,
                base.Owner.Creature, this);
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars["CrossSlashPower"].UpgradeValueBy(20);
        DynamicVars.Damage.UpgradeValueBy(6);
    }
}