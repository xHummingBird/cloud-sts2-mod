using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
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

public class Ascension() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), ILimitCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(42m, ValueProp.Move),
        new PowerVar<ArmorBreakPower>(80m),
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
            CinematicAttack.Start(RunManager.Instance.NetService.NetId);
            // cam?.StartCinematic(300f);
            await cloud.DashTo(ownerCreature, play.Target, distance: 500f);
            float duration = cloud.PlayAnimation(ownerCreature, "ascension").total;
            if (duration > 0f)
            {
                float[] hitTimings = new float[]
                {
                    0.18f, 0.567f, 1.033f, 1.57f, 2.1f,
                    2.433f // 14th hit
                };

                float chargeTime = 2.467f;
                float finalHitTime = 3.267f;


                float previousTime = 0f;

                // ✅ First 14 hits
                for (int i = 0; i < hitTimings.Length; i++)
                {
                    float delay = hitTimings[i] - previousTime;
                    previousTime = hitTimings[i];

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));
                    CloudExtensions.CombatHelpers.FakeHit(play.Target, "res://Cloud/sfx/sword_swing.wav");
                }
                
                {
                    cloud.DoScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
                    float delay = chargeTime - previousTime;
                    previousTime = chargeTime;

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));

                    SfxCmd.Play("res://Cloud/sounds/owarida.wav");
                }
                
                {
                    float delay = finalHitTime - previousTime;

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));
                    SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");

                    DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this)
                        .Targeting(play.Target)
                        .WithHitFx("vfx/vfx_attack_slash", "res://Cloud/sfx/cloud_hit.wav")
                        .Execute(choiceContext);
                    cloud.DoScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
                    await Task.Delay((int)(1.5f * 1000f));
                    // cam?.EndCinematic();
                    CinematicAttack.End(RunManager.Instance.NetService.NetId);
                    await cloud.Retreat(ownerCreature);
                    await PowerCmd.Apply<ArmorBreakPower>(choiceContext, play.Target,
                        base.DynamicVars["ArmorBreakPower"].BaseValue,
                        base.Owner.Creature, this);
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
                await PowerCmd.Apply<ArmorBreakPower>(choiceContext, play.Target,
                    base.DynamicVars["ArmorBreakPower"].BaseValue,
                    base.Owner.Creature, this);
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
            await PowerCmd.Apply<ArmorBreakPower>(choiceContext, play.Target,
                base.DynamicVars["ArmorBreakPower"].BaseValue,
                base.Owner.Creature, this);
        }
    }
    
    protected override void OnUpgrade()
    {
        base.DynamicVars["ArmorBreakPower"].UpgradeValueBy(20);
        DynamicVars.Damage.UpgradeValueBy(10);
    }
}