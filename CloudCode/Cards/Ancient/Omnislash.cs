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
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Omnislash() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), ILimitCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(14),
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
            SfxCmd.Play("res://Cloud/sounds/limit_break.wav");
            SfxCmd.Play("res://Cloud/sfx/limit_break_thunder.wav");
            cloud.PlayAnimation(ownerCreature, "limit_break_2");
            await Task.Delay((int)(1.0667f * 1000f));
            await cloud.DashTo(ownerCreature, play.Target, distance: 550f);
            float duration = cloud.PlayAnimation(ownerCreature, "omnislash").total;
            if (duration > 0f)
            {
                float[] hitTimings = new float[]
                {
                    0.067f, 0.567f, 0.933f, 1.4f, 1.933f, 2.633f,
                    3.333f, 3.867f, 4.233f, 4.6f, 5.067f,
                    5.167f, 5.5f, 5.8f // 14th hit
                };

                float chargeTime = 6.067f;
                float finalHitTime = 7.067f;


                float previousTime = 0f;

                // ✅ First 14 hits
                for (int i = 0; i < hitTimings.Length; i++)
                {
                    float delay = hitTimings[i] - previousTime;
                    previousTime = hitTimings[i];

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));

                    // ✅ Cloud voice (random)
                    AudioHelper.PlayRandomAttack();

                    // ✅ Sword SFX
                    if (i == hitTimings.Length - 1)
                    {
                        // 14th hit = heavy swing
                        SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
                    }
                    else
                    {
                        SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                    }

                    CommonActions.CardAttack(this, play.Target)
                        .WithHitFx("vfx/vfx_attack_slash")
                        .Execute(choiceContext);
                }

                // ✅ Charge phase
                {
                    float delay = chargeTime - previousTime;
                    previousTime = chargeTime;

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));

                    SfxCmd.Play("res://Cloud/sounds/koredeowarida.wav");
                    SfxCmd.Play("res://Cloud/sfx/energy_2.wav");
                    // your planned voice line fits perfectly here
                    // SfxCmd.Play("res://Cloud/voice/omnislash_charge.wav");
                }

                // ✅ Final hit
                {
                    float delay = finalHitTime - previousTime;

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));

                    // ✅ Final hit voice (optional but feels good)
                     // or dedicated final voice
                    // ✅ Final hit SFX
                    SfxCmd.Play("res://Cloud/sfx/omnislash_finalhit.wav");

                    DamageCmd.Attack(base.DynamicVars.Damage.BaseValue*2).FromCard(this).Targeting(play.Target)
                        .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                        .Execute(choiceContext);
                    await Task.Delay((int)(0.8f * 1000f));
                    await cloud.Retreat(ownerCreature);
                }

            }
            else
            {
                await CommonActions.CardAttack(this, play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue*2).FromCard(this).Targeting(play.Target)
                    .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                    .Execute(choiceContext);
            }
        }
        else
        {
            await CommonActions.CardAttack(this, play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue*2).FromCard(this).Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                .Execute(choiceContext);
        }
        
        
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}