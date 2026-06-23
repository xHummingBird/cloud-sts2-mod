using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Stance;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class CrossSlash() : CloudCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IATBCard
{
    public int ATBCost => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(18, ValueProp.Move),
        new PowerVar<CrossSlashPower>(50),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);   
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            await cloud.DashTo(ownerCreature, play.Target, distance: 550f);
            float duration = cloud.PlayAnimation(ownerCreature, "cross_slash").total;
            if (duration > 0f)
            {
                SfxCmd.Play("res://Cloud/sounds/heavy_attack (3).wav");
                await Task.Delay((int)(0.85f * 1000f));
                CloudExtensions.CombatHelpers.FakeHit(play.Target);
                
                await Task.Delay((int)(0.60f * 1000f));
                CloudExtensions.CombatHelpers.FakeHit(play.Target);
                
                await Task.Delay((int)(0.80f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
                SfxCmd.Play("res://Cloud/sounds/ungawarukatana.wav");
                CommonActions.CardAttack(this, play.Target)
                    .WithHitFx("vfx/vfx_attack_slash",
                        "res://Cloud/sfx/cloud_hit.wav")
                    .Execute(choiceContext);
                NGame.Instance.ScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
                
                await Task.Delay((int)(0.20f * 1000f));
                if (play.Target != null)
                {
                    await PowerCmd.Apply<CrossSlashPower>(choiceContext, play.Target,
                        base.DynamicVars["CrossSlashPower"].BaseValue,
                        base.Owner.Creature, this);
                }

                await Task.Delay((int)(0.60f * 1000f));
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
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        base.DynamicVars["CrossSlashPower"].UpgradeValueBy(20);
    }
}