using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class SteadfastBlock() : CloudCard(2, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
protected override IEnumerable<DynamicVar> CanonicalVars => [
    new BlockVar(12m, ValueProp.Move),
    new EnergyVar(1)
];
    
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    var ownerCreature = Owner?.Creature;

    if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
    {
        AudioHelper.PlayRandomDefend();
    }
    decimal amount = await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
    await PowerCmd.Apply<AtbNextTurnPower>(choiceContext, base.Owner.Creature, DynamicVars.Energy.BaseValue, base.Owner.Creature, this);
    if (base.Owner.Creature.IsPunisher())
    {
        await PowerCmd.Remove<PunisherModePower>(base.Owner.Creature);
    }
        
}

protected override void OnUpgrade()
{
    base.DynamicVars.Energy.UpgradeValueBy(1m);
}
}