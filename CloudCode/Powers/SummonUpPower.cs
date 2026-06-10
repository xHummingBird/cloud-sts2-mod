using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Cloud.CloudCode.Powers;

public class SummonUpPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        var card = cardPlay.Card;
        if (card.Owner.Creature != Owner) return;
        var player = Owner;

        bool isMagic = card is IMagicCard;

        if (card.Type == CardType.Attack || isMagic)
        {
            SummonManager.GainSummon(player.Player, isMagic ? 4 : 2);
        }
    }
}