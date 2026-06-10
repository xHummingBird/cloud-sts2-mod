using BaseLib.Extensions;
using BaseLib.Utils;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class OmnislashVerFive() : CloudCard(2, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), ILimitCard, IATBCard
{
    
    protected override bool ShouldGlowGoldInternal =>
        LimitManager.GetLimit(base.Owner) >= 50;
    
    private IEnumerable<CardModel> GetLimitBreakCards()
    {
        var pile = PileType.Hand.GetPile(base.Owner);
        return pile.Cards.OfType<LimitBreak>();
    }
    
    public int ATBCost => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(4m),
        new ExtraDamageVar(2m),
        new RepeatVar(6),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) =>
                LimitManager.GetLimit(card.Owner) >= 50 ? 1 : 0)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (LimitManager.GetLimit(base.Owner) >= 50)
        {
            LimitManager.SpendLimit(base.Owner,50);
        }

        if (base.Owner.HasPower<LimitBreakPower>())
        {
            foreach (var card in GetLimitBreakCards().ToList())
            {
                await CardCmd.Exhaust(choiceContext, card);
            }
            await PowerCmd.Remove<LimitBreakPower>(base.Owner.Creature);
        }
        
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            var combat = CombatState ?? Owner.Creature.CombatState;

            bool noOtherEnemies =
                !combat.Enemies.Any(e => e != play.Target && !e.IsDead);

            decimal totalDamage = DynamicVars.CalculatedDamage.PreviewValue * 6;
            decimal damage = DynamicVars.CalculatedDamage.PreviewValue;

            bool specialAnimation =
                noOtherEnemies && totalDamage >= play.Target.CurrentHp;

            if (specialAnimation)
            {
                SfxCmd.Play("res://Cloud/sounds/omnislashver5_start.wav");
                damage = 0;
            }
            else SfxCmd.Play("res://Cloud/sounds/omnislashver5_1.wav");
            SfxCmd.Play("res://Cloud/sfx/limit_break_thunder.wav");
            cloud.PlayAnimation(ownerCreature, "limit_break_1");
            await Task.Delay((int)(1.2f * 1000f));
            await cloud.DashTo(ownerCreature, play.Target, distance: 550f);
            float duration = cloud.PlayAnimation(ownerCreature, "omnislash_ver_5").total;
            if (duration > 0f)
            {
                float[] hitTimings = new float[]
                {
                    0.100f, 0.370f, 0.570f, 0.670f, 0.97f // 14th hit
                };

                float chargeTime = 1.20f;
                float finalHitTime = 1.80f;


                float previousTime = 0f;

                // ✅ First 14 hits
                for (int i = 0; i < hitTimings.Length; i++)
                {
                    float delay = hitTimings[i] - previousTime;
                    previousTime = hitTimings[i];

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));

                    // ✅ Cloud voice (random)
                    

                    // ✅ Sword SFX
                    
                    DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target)
                        .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                        .Execute(choiceContext);
                }

                // ✅ Charge phase
                {
                    float delay = chargeTime - previousTime;
                    previousTime = chargeTime;

                    if (delay > 0f)
                        await Task.Delay((int)(delay * 1000f));
                    
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
                    cloud.DoScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
                    if (specialAnimation)
                    {
                        await Task.Delay((int)(0.2f * 1000f));
                        SfxCmd.Play("res://Cloud/sounds/omnislashver5_end.wav");
                        await Task.Delay((int)(1.03f * 1000f));
                        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(play.Target)
                            .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                            .Execute(choiceContext);
                    }
                    else
                    {
                        DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this).Targeting(play.Target)
                            .WithHitFx("vfx/vfx_attack_slash") // swap for bigger VFX later
                            .Execute(choiceContext);
                        await Task.Delay((int)(0.2f * 1000f));
                        SfxCmd.Play("res://Cloud/sounds/warukuomouna.wav");
                        await Task.Delay((int)(1.03f * 1000f));
                        await cloud.Retreat(ownerCreature);
                    }
                }

            }
            else
            {
                await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this).Targeting(play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
            }
        }
        else
        {
            await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this).Targeting(play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
