using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Extensions;

public static class CloudExtensions
{
    public static bool IsPunisher(this Creature creature)
        => creature.HasPower<PunisherModePower>();

    public static bool IsOperator(this Creature creature)
        => !creature.IsPunisher();
    
    public static bool IsPrime(this Creature creature)
        => creature.HasPower<PrimeModePower>();

    public static async Task TogglePunisher(this Creature creature,
        decimal amount,
        Creature source,
        CardModel card)
    {
        if (creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Remove<PunisherModePower>(creature);
        }
        else
        {
            await PowerCmd.Apply<PunisherModePower>(new ThrowingPlayerChoiceContext(), creature, 1, source, card);
        }
    }

    public static async Task EnterPunisher(this Creature creature,
        decimal amount,
        Creature source,
        CardModel card)
    {
        if (!creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Apply<PunisherModePower>(new ThrowingPlayerChoiceContext(),creature, 1, source, card);
        }
    }

    public static async Task ExitPunisher(this Creature? creature)
    {
        if (creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Remove<PunisherModePower>(creature);
        }
    }
    
    
   
    public static async Task CheckLimitReady(
        this Creature creature,
        PlayerChoiceContext context,
        Creature source,
        CardModel? card)
    {
        var player = creature.Player;
        if (player == null)
            return;

        if (LimitManager.IsFull(player) &&
            !creature.HasPower<LimitBreakPower>())
        {
            await PowerCmd.Apply<LimitBreakPower>(
                context,
                creature,
                1,
                creature,
                null
            );
        }
    }
    
    public static async Task CheckSummonReady(
        this Creature creature,
        PlayerChoiceContext context,
        Creature source,
        CardModel? card)
    {
        var player = creature.Player;
        if (player == null)
            return;

        if (SummonManager.IsFull(player) &&
            !creature.HasPower<SummonPower>())
        {
            await PowerCmd.Apply<SummonPower>(
                context,
                creature,
                1,
                creature,
                null
            );
        }
    }

}
