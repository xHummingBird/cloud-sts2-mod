using BaseLib.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class PunisherModePower : CloudPower
{
    private const string _damageIncrease = "DamageIncrease";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DamageIncrease", 1.35m),
        new DynamicVar("DamageTakenIncrease", 1.1m),
        new DynamicVar("PrimeDamageIncrease", 1.6m),
    ];
    
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var creature = base.Owner?.Player?.Creature;
        
        if (creature.Player?.Character is Character.Cloud character)
        {
            character.RefreshIdle(creature);
        }
        
        if (applier.HasPower<CombatMomentumPower>())
        {
            await Task.Delay((int)(0.50f * 1000f));
            decimal powerAmount = applier.GetPowerAmount<CombatMomentumPower>();
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), applier,
                powerAmount, null, null);
        }
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        var creature = oldOwner.Player?.Creature;
        
        
        if (creature?.IsDead == true)
            return;

        if (creature.Player?.Character is Character.Cloud character)
        {
            character.RefreshIdle(creature);
        }
    }
    
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!props.IsPoweredAttack())
            return 1m;

        if (cardSource is IMagicCard)
            return 1m;
        
        if (cardSource is ISummonCard)
            return 1m;
        
        decimal num = base.DynamicVars["DamageIncrease"].BaseValue;
        decimal num2 = base.DynamicVars["PrimeDamageIncrease"].BaseValue;
        decimal damageTaken =  base.DynamicVars["DamageTakenIncrease"].BaseValue;
        if (dealer == base.Owner)
        {
            if (dealer.HasPower<PrimeModePower>())
                return num2;
            else return num;
        }

        if (target == base.Owner)
            return damageTaken;

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