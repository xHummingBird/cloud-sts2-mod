using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
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
    CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1m, ValueProp.Move),
        new RepeatVar(14),
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
            float duration = cloud.PlayAnimation(ownerCreature, "omnislash").total;
            if (duration > 0f)
            {
                float[] hitTimings = new float[]
                {
                    1.133f, 1.633f, 2.0f, 2.466f, 3.0f, 3.7f,
                    4.4f, 4.933f, 5.3f, 5.666f, 6.133f,
                    6.233f, 6.566f, 6.866f // 14th hit
                };

                float chargeTime = 7.133f;
                float finalHitTime = 8.133f;

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

                    await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue*3).FromCard(this).Targeting(play.Target)
                        .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                        .Execute(choiceContext);
                }

            }
            else
            {
                await CommonActions.CardAttack(this, play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue*3).FromCard(this).Targeting(play.Target)
                    .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                    .Execute(choiceContext);
            }
        }
        else
        {
            await CommonActions.CardAttack(this, play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue*3).FromCard(this).Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                .Execute(choiceContext);
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}