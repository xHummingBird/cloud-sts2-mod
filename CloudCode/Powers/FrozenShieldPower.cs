using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class FrozenShieldPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override async Task AfterBlockCleared(Creature creature)
    {
        if (creature == base.Owner)
        {
            Flash();
            await CreatureCmd.GainBlock(base.Owner, 10, ValueProp.Unpowered, null);
            await PowerCmd.Decrement(this);
        }
    }
}