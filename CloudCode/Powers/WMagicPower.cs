using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Powers;

public class WMagicPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != base.Owner)
            return playCount;

        if (card is not IMagicCard)
            return playCount;

        int numMagicPlayedThisTurn = CombatManager.Instance.History.CardPlaysStarted.Count(
            e =>
                e.Actor == base.Owner &&
                e.CardPlay.IsFirstInSeries &&
                e.HappenedThisTurn(base.CombatState) &&
                e.CardPlay.Card is IMagicCard
        );

        if (numMagicPlayedThisTurn >= base.Amount)
            return playCount;

        return playCount + 1;
    }


    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        return Task.CompletedTask;
    }
}