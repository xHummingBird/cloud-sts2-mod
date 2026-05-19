using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Basic;

public class TestLimitBreak() : CloudCard(0, CardType.Skill,
    CardRarity.Basic, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move),
        new PowerVar<LimitBreakPower>(100)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var power = base.Owner.Creature.GetPower<LimitBreakPower>();
        
        if (power != null)
        {
            await power.AddLimitExternal((int)DynamicVars["LimitBreakPower"].BaseValue, choiceContext);
        }

        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(5m);
    }
}