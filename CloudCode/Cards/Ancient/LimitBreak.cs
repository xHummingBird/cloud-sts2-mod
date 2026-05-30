using BaseLib.Extensions;
using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class LimitBreak() : CloudCard(1, CardType.Skill,
    CardRarity.Ancient, TargetType.AnyEnemy), IATBCard, ILimitCard
{
    protected override bool ShouldGlowGoldInternal => IsPlayable;
    protected override bool IsPlayable => base.Owner.HasPower<LimitBreakPower>();
    public int ATBCost => 3;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];
    
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (IsUpgraded)
            {
                return new IHoverTip[]
                {
                    HoverTipFactory.FromCard<CrossSlashKai>(true),
                    HoverTipFactory.FromCard<Meteorain>(true),
                    HoverTipFactory.FromCard<Omnislash>()
                };
            }

            return new IHoverTip[]
            {
                HoverTipFactory.FromCard<CrossSlashKai>(),
                HoverTipFactory.FromCard<Meteorain>(),
                HoverTipFactory.FromCard<Ascension>()
            };
        }
    }


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var cross = CombatState.CreateCard<CrossSlashKai>(base.Owner);
        var meteor = CombatState.CreateCard<Meteorain>(base.Owner);
        
        List<CardModel> cards;
        
        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(cross);
            CardCmd.Upgrade(meteor);

            cards = new()
            {
                cross,
                meteor,
                CombatState.CreateCard<Omnislash>(base.Owner)
            };
        }

        else
        {
            cards = new()
            {
                cross,
                meteor,
                CombatState.CreateCard<Ascension>(base.Owner)
            };
        }
        CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards.ToList(), base.Owner, canSkip: false);
        if (cardModel is Meteorain meteorain)
            await CardCmd.AutoPlay(choiceContext, cardModel, null);
        
        else await CardCmd.AutoPlay(choiceContext, cardModel, play.Target);
    }

    protected override void OnUpgrade()
    {
        
    }
}

    
    
