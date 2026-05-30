using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class PunisherModePower : CloudPower
{
    private const string _damageIncrease = "DamageIncrease";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DamageIncrease", 1.2m),
        new DynamicVar("PrimeDamageIncrease", 1.4m)
    ];

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack())
            return 1m;

        if (cardSource is IMagicCard)
            return 1m;

        decimal num = base.DynamicVars["DamageIncrease"].BaseValue;
        decimal num2 = base.DynamicVars["PrimeDamageIncrease"].BaseValue;
        if (dealer == base.Owner)
        {
            if (dealer.HasPower<PrimeModePower>())
                return num2;
            else return num;
        }

        if (target == Owner)
        {
            if (target.HasPower<FuryPower>())
                return num2;
            else return num;
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

        if (target.HasPower<PrimeModePower>())
            return;

        if (result.UnblockedDamage <= 0)
            return;
        
        if (dealer == null || !dealer.IsEnemy)
            return;

        await PowerCmd.Remove(this);
    }
}