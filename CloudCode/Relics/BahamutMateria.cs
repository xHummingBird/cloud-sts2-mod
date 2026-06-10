using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;

namespace Cloud.CloudCode.Relics;

public class BahamutMateria() : CloudRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Bahamut>(),
    ];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Creature.Side)
            return;
        if (combatState.RoundNumber <= 1)
        {
            SummonManager.GainSummon(Owner.Creature.Player, 20);
        }
        Flash();
    }
}