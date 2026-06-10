using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Mechanics.ATB;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Relics;

public class FusionSword() : CloudRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<OmnislashVerFive>()
    ];
    
    public override bool HasUponPickupEffect => true;
    
    public override async Task AfterObtained()
    {
        CardModel card = base.Owner.RunState.CreateCard<OmnislashVerFive>(base.Owner);
        CardCmd.PreviewCardPileAdd((await CardPileCmd.Add(card, PileType.Deck)), 2f);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Creature.Side)
            return;
        if (combatState.RoundNumber <= 1)
        {
            ATBManager.SetATB(base.Owner, 3);
        }
    }
}