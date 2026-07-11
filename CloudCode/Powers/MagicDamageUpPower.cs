using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class MagicDamageUpPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        
        if (cardSource is Odin)
        {
            return 1m;
        }

        decimal num = 1m + (Amount / 100m);
        
        if (dealer == base.Owner && cardSource is IMagicCard magicCard)
        {
            return num;
        }
        
        if (dealer == base.Owner && cardSource is ISummonCard summonCard)
        {
            return num;
        }

        return 1m;
    }
}