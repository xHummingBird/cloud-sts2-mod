using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Cloud.CloudCode.Relics;

public class BusterSword() : CloudRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<LimitBreakPower>(),
        HoverTipFactory.FromCard<LimitBreak>()
    ];

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        
        if (side != base.Owner.Creature.Side)
            return;

        var creature = base.Owner.Creature;

        if (creature.GetPowerAmount<LimitBreakPower>() >= 100)
            return;

        Flash();
        var power = creature.GetPower<LimitBreakPower>();
        
        if (power != null)
        {
            await power.AddLimitExternal(5, new ThrowingPlayerChoiceContext());
        }
        
        else
        {
            // ✅ First-time creation path
            await PowerCmd.Apply<LimitBreakPower>(
                new ThrowingPlayerChoiceContext(),
                creature,
                5,
                creature,
                null
            );
        }


    }
}