using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class LimitBreakPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SfxCmd.Play("res://Cloud/sounds/limit_break_2.wav");
        // CardModel card = CombatState.CreateCard<LimitBreak>(base.Owner.Player);
        // ChampionBelt? championBelt = base.Owner?.Player.GetRelic<ChampionBelt>();
        // if (championBelt != null)
        // {
        //     CardCmd.Upgrade(card);
        // }
        // await Task.Delay((int)(0.50f * 1000f));
        // await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner.Player);
        Flash();
    }
    // public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
    //     ICombatState combatState)
    // {
    //     if (side != base.Owner.Side)
    //         return;
    //
    //     var player = Owner.Player;
    //     var playerState = player.PlayerCombatState;
    //
    //     if (playerState == null)
    //         return;
    //     
    //     if (playerState.AllCards
    //         .OfType<LimitBreak>()
    //         .Any(c => c.Pile?.Type == PileType.Hand))
    //     {
    //         return;
    //     }
    //
    //     var cards = playerState.AllCards
    //         .OfType<LimitBreak>()
    //         .Where(c => c.Pile == null || c.Pile.Type != PileType.Hand);
    //         await CardPileCmd.Add(cards, PileType.Hand);
    // }
}