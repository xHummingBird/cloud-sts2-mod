using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Omnislash() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), ILimitCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(70m, ValueProp.Move),
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
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            SfxCmd.Play("res://Cloud/sounds/limit_break.wav");
            SfxCmd.Play("res://Cloud/sfx/limit_break_thunder.wav");
            cloud.PlayAnimation(ownerCreature, "limit_break_2");
            await Task.Delay((int)(1.0667f * 1000f));
            
            // cam?.StartCinematic(300f);
            
            await cloud.DashTo(ownerCreature, play.Target, distance: 500f);
            float duration = cloud.PlayAnimation(ownerCreature, "omnislash").total;
            if (duration > 0f)
            {
                float[] hitTimings = new float[]
                {
                    0.067f, 0.567f, 0.933f, 1.4f, 1.933f, 2.633f,
                    3.333f, 3.867f, 4.233f, 4.6f, 5.067f,
                    5.167f, 5.5f, 5.8f
                };

                float chargeTime = 6.067f;
                float finalHitTime = 7.067f;


                float previousTime = 0f;
                
                for (int i = 0; i < hitTimings.Length; i++)
                {
                    float delay = hitTimings[i] - previousTime;
                    previousTime = hitTimings[i];

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));
                    
                    AudioHelper.PlayRandomAttack();
                    
                    if (i == hitTimings.Length - 1)
                    {
                        CloudExtensions.CombatHelpers.FakeHit(play.Target);
                    }
                    else
                    {
                        CloudExtensions.CombatHelpers.FakeHit(play.Target, "res://Cloud/sfx/sword_swing.wav");
                    }
                }
                
                {
                    float delay = chargeTime - previousTime;
                    previousTime = chargeTime;

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));
                    SfxCmd.Play("res://Cloud/sounds/koredeowarida.wav");
                    SfxCmd.Play("res://Cloud/sfx/energy_2.wav");
                }
                
                {
                    float delay = finalHitTime - previousTime;

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));
                    
                    SfxCmd.Play("res://Cloud/sfx/omnislash_finalhit.wav");
                    cloud.DoScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
                    DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
                        .WithHitFx("vfx/vfx_attack_slash", "res://Cloud/sfx/cloud_hit.wav") // swap for bigger VFX later
                        .Execute(choiceContext);
                    await Task.Delay((int)(0.8f * 1000f));
                    // cam?.EndCinematic();
                    CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
                    await cloud.Retreat(ownerCreature);
                }

            }
            else
            {
                await CommonActions.CardAttack(this, play.Target)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
            }
        }
        else
        {
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        
        
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(15);
    }
}