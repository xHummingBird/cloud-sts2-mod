using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Common;

public class GotYourBack() : CloudCard(2, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move),
        new PowerVar<LimitBreakPower>(5)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        
        LimitManager.GainLimit(Owner, DynamicVars["LimitBreakPower"].IntValue);
        await Owner.Creature.CheckLimitReady(
            choiceContext,
            Owner.Creature,
            null
        );

        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);
        
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(4m);
    }
}