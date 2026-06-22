using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class PrimeModePower : CloudPower
{
    private const string _magicDamageIncrease = "MagicDamageIncrease";
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("MagicDamageIncrease", 1.25m),
    ];

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        
        if (cardSource is Odin)
        {
            return 1m;
        }
        
        decimal num = base.DynamicVars["MagicDamageIncrease"].BaseValue;
        if (dealer == base.Owner && cardSource is IMagicCard)
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