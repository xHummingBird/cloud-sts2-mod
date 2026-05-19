using Cloud.CloudCode.Cards.Ancient;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class LimitBreakPower : CloudPower
{
public override PowerType Type => PowerType.Buff;

public override PowerStackType StackType => PowerStackType.Counter;

private async Task AddLimit(int amount, PlayerChoiceContext choiceContext)
{
    
    var creature = base.Owner;
    int current = creature.GetPowerAmount<LimitBreakPower>();

    // ✅ Stop at 100
    if (current >= 100)
        return;

    int final = Math.Min(current + amount, 100);
    int gain = final - current;

    if (gain <= 0)
        return;
    // ✅ Apply Limit
    await PowerCmd.Apply<LimitBreakPower>(choiceContext,
        creature,
        gain,
        creature,
        null );
    if (final == 100)
    {
        await OnLimitReached();
    }
}

public async Task AddLimitExternal(int amount, PlayerChoiceContext context)
{
    await AddLimit(amount, context);
}

private bool _limitCardGenerated = false;

private async Task OnLimitReached()
{
    if (_limitCardGenerated)
        return;
    
    _limitCardGenerated = true;
    
    var player = base.Owner.Player;
    CardModel card = CombatState.CreateCard<LimitBreak>(base.Owner.Player);
    await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner.Player); 
}

public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
{
    if (target != base.Owner)
        return;

    if (dealer == base.Owner)
        return;
    
    if (!props.IsPoweredAttack())
        return;
    
    int gain = 0;
    
    if (result.BlockedDamage > 0)
    {
        gain += 3;
    }
    
    if (result.UnblockedDamage > 0)
    {
        gain += result.UnblockedDamage;
    }
    
    if (gain > 0)
    {
        await AddLimit(gain, choiceContext);
    }
}
}