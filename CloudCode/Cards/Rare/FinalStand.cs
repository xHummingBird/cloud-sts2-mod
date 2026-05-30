using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class FinalStand() : CloudCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IATBCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [
            new PowerVar<ReprievePower>(1m),
        ];
    public int ATBCost => (int)(4 - DynamicVars["ReprievePower"].BaseValue);
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        PowerCmd.Apply<ReprievePower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ReprievePower"].UpgradeValueBy(1m);
    }
}