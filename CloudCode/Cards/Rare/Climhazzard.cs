using BaseLib.Utils;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class Climhazzard() : CloudCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IATBCard
{
    public int ATBCost => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move),
        new PowerVar<ArmorBreakPower>(50m),
        new RepeatVar(3)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            await cloud.DashTo(ownerCreature, play.Target, distance: 550f);
            float duration = cloud.PlayAnimation(ownerCreature, "climhazzard").total;
            if (duration > 0f)
            {
                SfxCmd.Play("res://Cloud/sounds/sokoda.wav");
                await Task.Delay((int)(0.1f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
                CommonActions.CardAttack(this, play.Target)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                
                await Task.Delay((int)(0.3f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
                CommonActions.CardAttack(this, play.Target)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                
                await Task.Delay((int)(0.65f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
                SfxCmd.Play("res://Cloud/sounds/owarida.wav");
                CommonActions.CardAttack(this, play.Target)
                    .WithHitFx("vfx/vfx_attack_slash",
                        "res://Cloud/sfx/omnislash_finalhit.wav")
                    .Execute(choiceContext);
                if (play.Target != null)
                {
                    await PowerCmd.Apply<ArmorBreakPower>(choiceContext, play.Target,
                        base.DynamicVars["ArmorBreakPower"].BaseValue,
                        base.Owner.Creature, this);
                }
                await Task.Delay((int)(0.70f * 1000f));
                await cloud.Retreat(ownerCreature);
            }
        }
        else
        {
            await CommonActions.CardAttack(this, play.Target).WithHitCount(base.DynamicVars.Repeat.IntValue)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            if (play.Target != null)
                await PowerCmd.Apply<ArmorBreakPower>(choiceContext, play.Target,
                    base.DynamicVars["ArmorBreakPower"].BaseValue,
                    base.Owner.Creature, this);
        }
    }
    
    protected override void OnUpgrade()
    {
        base.DynamicVars["ArmorBreakPower"].UpgradeValueBy(20);
    }
}