using Cloud.CloudCode.Mechanics.ATB;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Powers;

public class CombatMomentumPower : CloudPower
{
    public bool switchPunisher = false;
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (!(amount <= 0) && power is PunisherModePower)
        {
            switchPunisher = true;
            var ownerPlayer = base.Owner.Player;
            Flash();
            if (ownerPlayer != null)
                await PowerCmd.Apply<StrengthPower>(choiceContext, ownerPlayer.Creature, Amount, ownerPlayer.Creature, null);
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side)
            return;
        switchPunisher = false;
    }

}