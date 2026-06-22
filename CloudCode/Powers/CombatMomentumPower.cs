using Cloud.CloudCode.Mechanics.ATB;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Powers;

public class CombatMomentumPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power is not PunisherModePower)
            return;
        
        var history = CombatManager.Instance?.History;
        if (history == null)
            return;
        
        int triggersThisTurn = history.Entries
            .OfType<PowerReceivedEntry>()
            .Count(e =>
                e.Power is PunisherModePower &&
                e.Power.Owner == Owner &&
                e.HappenedThisTurn(CombatState)
            );
        
        if (triggersThisTurn > 1)
            return;

        if (!(amount <= 0m) && applier == base.Owner && power is PunisherModePower)
        {
            Flash();
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, base.Amount, null, null);
        }
    }
}

