using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class Manawall() : CloudCard(2, CardType.Skill,
    CardRarity.Rare, TargetType.Self), IATBCard, IMagicCard
{
    public int ATBCost => 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12m, ValueProp.Move),
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            // attack animation
            float duration = cloud.PlayAnimation(ownerCreature, "cast").total;
            AudioHelper.PlayRandomDefend();
            // Optional: delay to sync hit roughly mid animation
            if (duration > 0f)
                await Task.Delay((int)(duration * 0.2f * 1000f));
        }
        decimal amount = await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}