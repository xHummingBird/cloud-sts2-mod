using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class ArmorBreakPower : CloudPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        
        if (dealer == base.Owner)
        {
            return 1m;
        }

        if (target == Owner)
        {
            return (1m + amount / 100);
        }

        return 1m;
        
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // Guard clauses: fail fast
        if (!CombatManager.Instance.IsInProgress)
            return;

        if (target != base.Owner)
            return;
        
        if (result.UnblockedDamage <= 0)
            return;
        
        if (dealer == null || !dealer.IsEnemy)
            return;

        await PowerCmd.Remove(this);
    }
}
