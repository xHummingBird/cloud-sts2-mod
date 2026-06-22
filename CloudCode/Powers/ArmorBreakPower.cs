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

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner)
        {
            return 1m;
        }
        
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        
        return (1m + (Amount / 100m));
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != base.Owner)
            return;
        
        if (!props.IsPoweredAttack())
        {
            return;
        }
        
        if (result.UnblockedDamage <= 0)
            return;
        
        if (dealer == null)
            return;

        await PowerCmd.Remove(this);
    }
}
