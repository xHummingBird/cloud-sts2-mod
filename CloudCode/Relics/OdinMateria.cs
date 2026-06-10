using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Relics;

public class OdinMateria() : CloudRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Odin>(),
        HoverTipFactory.FromCard<Zantetsuken>()
    ];
    
    public override bool HasUponPickupEffect => true;
    
    public override async Task AfterObtained()
    {
        CardModel card = base.Owner.RunState.CreateCard<Zantetsuken>(base.Owner);
        CardCmd.PreviewCardPileAdd((await CardPileCmd.Add(card, PileType.Deck)), 2f);
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Creature.Side)
            return;
        if (combatState.RoundNumber <= 1)
        {
            await PowerCmd.Apply<PunisherModePower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1m, null,
                null);
        }
        Flash();
    }
}