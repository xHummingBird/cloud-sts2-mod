using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Powers;

public class SummonPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        CardModel card = CombatState.CreateCard<SummonCard>(base.Owner.Player);
        if (base.Owner.HasPower<SummonUpPower>())
        {
            CardCmd.Upgrade(card);
        }
        SfxCmd.Play("res://Cloud/sounds/summon_choose.wav");
        await Task.Delay((int)(0.50f * 1000f));
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner.Player);
        Flash();
    }
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side)
            return;

        if (base.Owner.HasPower<MegaflarePower>())
            return;

        var player = Owner.Player;
        var playerState = player.PlayerCombatState;

        if (playerState == null)
            return;
        
        if (playerState.AllCards
            .OfType<SummonCard>()
            .Any(c => c.Pile?.Type == PileType.Hand))
        {
            return;
        }

        var cards = playerState.AllCards
            .OfType<SummonCard>()
            .Where(c => c.Pile == null || c.Pile.Type != PileType.Hand);
        await CardPileCmd.Add(cards, PileType.Hand);
    }
}