using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Cloud.CloudCode.Powers;

public class HastePower : CloudPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side)
            return;

        var player = Owner.Player;
        if (player != null)
        {
            Flash();
            ATBManager.GainATBDirect(player, 1);
        }
    }
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext playerChoiceContext, Player player)
    {
        if (player == base.Owner.Player)
        {
            await CardPileCmd.Draw(playerChoiceContext, 1, base.Owner.Player);
            await PowerCmd.Decrement(this);
        }
    }
}