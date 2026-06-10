using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class ThundagaBurst() : CloudCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.RandomEnemy), IATBCard, IMagicCard
{
    public int ATBCost => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(7m, ValueProp.Move),
        new RepeatVar(3)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
       
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            // attack animation
            float duration = cloud.PlayAnimation(ownerCreature, "cast").total;
            AudioHelper.PlayRandomThunder();
            // Optional: delay to sync hit roughly mid animation
            if (duration > 0f)
                await Task.Delay((int)(0.2f * 1000f));
        }
        
        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(base.DynamicVars.Repeat.IntValue).FromCard(this)
            .TargetingRandomOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_attack_lightning", "event:/sfx/characters/defect/defect_lightning_passive")
            .Execute(choiceContext);
        
        bool killedSomething = attackCommand.Results
            .SelectMany(r => r)
            .Any(r => r.WasTargetKilled);
        
        bool shouldTriggerFatal = CombatState.Enemies
            .Where(e => !e.IsDead)
            .All(e => e.Powers.All(p => p.ShouldOwnerDeathTriggerFatal()));
        
        if (killedSomething && shouldTriggerFatal)
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard(this)
                .TargetingRandomOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_lightning", "event:/sfx/characters/defect/defect_lightning_passive")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}