using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Relics;

public class RuneBlade() : CloudRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;
    
    private const string _damageIncrease = "DamageIncrease";
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DamageIncrease", 1.2m),
    ];
    
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack())
            return 1m;
        
        if (base.Owner.Creature.HasPower<PunisherModePower>())
            return 1m;
        
        decimal num = base.DynamicVars["DamageIncrease"].BaseValue;
        if (cardSource is IMagicCard)
            return num;
        
        if (cardSource is ISummonCard)
            return num;
        
        return 1m;
    }
}