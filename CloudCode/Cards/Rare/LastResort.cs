using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Cards.Rare;

public class LastResort() : CloudCard (2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        List<CardModel> list = PileType.Hand.GetPile(base.Owner).Cards.ToList();
        foreach (CardModel item in list)
        {
            await CardCmd.Exhaust(choiceContext, item);
        }
        await PowerCmd.Apply<NoBlockPower>(choiceContext, base.Owner.Creature, 2, base.Owner.Creature, this);
        LimitManager.GainLimit(Owner, 100);
        await Owner.Creature.CheckLimitReady(
            choiceContext,
            Owner.Creature,
            null);
        ATBManager.GainATBDirect(base.Owner, 3);
    }
}