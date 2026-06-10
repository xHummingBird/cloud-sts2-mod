using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Common;

public class Siphon() : CloudCard(1, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5, ValueProp.Move),
        new PowerVar<SummonPower>(5)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SummonPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), context: choiceContext, player: base.Owner, filter: null, source: this)).FirstOrDefault();
        if (cardModel != null)
        {
            await CardCmd.Exhaust(choiceContext, cardModel);
        }
        SummonManager.GainSummon(Owner, DynamicVars["SummonPower"].IntValue);
        await Owner.Creature.CheckSummonReady(
            choiceContext,
            Owner.Creature,
            null
        );
        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, cardPlay);
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(2m);
        DynamicVars["SummonPower"].UpgradeValueBy(2m);
    }
    
}