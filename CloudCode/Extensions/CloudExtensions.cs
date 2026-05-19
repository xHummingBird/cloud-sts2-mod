using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Extensions;

public static class CloudExtensions
{
    public static bool IsPunisher(this Creature creature)
        => creature.HasPower<PunisherModePower>();

    public static bool IsOperator(this Creature creature)
        => !creature.IsPunisher();

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

    public static async Task ExitPunisher(this Creature creature)
    {
        if (creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Remove<PunisherModePower>(creature);
        }
    }
}
