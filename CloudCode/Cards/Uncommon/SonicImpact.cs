using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class SonicImpact() : CloudCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(11, ValueProp.Move),
        new EnergyVar(1),
    ];
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        
        var enemies = CombatState.Enemies;
        bool shouldTriggerFatal = enemies
            .Where(e => !e.IsDead)
            .All(enemy =>
                enemy.Powers.All(p => p.ShouldOwnerDeathTriggerFatal()));

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            AudioHelper.PlayRandomAttack();
            float duration = cloud.PlayAnimation(ownerCreature, "sonic_impact").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.133f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
                await Task.Delay((int)(0.300f * 1000f));
            }
        }
        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (shouldTriggerFatal && attackCommand.Results.SelectMany((List<DamageResult> r) => r)
                .Any((DamageResult r) => r.WasTargetKilled))
        {
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}