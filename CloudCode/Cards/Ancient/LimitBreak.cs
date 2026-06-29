using BaseLib.Extensions;
using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using Cloud.CloudCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class LimitBreak() : CloudCard(0, CardType.Skill,
    CardRarity.Ancient, TargetType.AnyEnemy), ILimitCard
{
    protected override bool ShouldGlowGoldInternal => IsPlayable;
    protected override bool IsPlayable => base.Owner.HasPower<LimitBreakPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var cross = CombatState.CreateCard<CrossSlashKai>(base.Owner);
        var meteor = CombatState.CreateCard<Meteorain>(base.Owner);
        var ascension = CombatState.CreateCard<Ascension>(base.Owner);
        var omnislash = CombatState.CreateCard<Omnislash>(base.Owner);
        
        UltimaWeapon? ultimaWeapon = base.Owner?.GetRelic<UltimaWeapon>();
        
        List<CardModel> cards;
        
        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(cross);
            CardCmd.Upgrade(meteor);
            CardCmd.Upgrade(ascension);
            CardCmd.Upgrade(omnislash);

            cards = new()
            {
                cross,
                meteor,
                ascension
            };
        }

        else
        {
            cards = new()
            {
                cross,
                meteor,
                ascension
            };
        }

        if (ultimaWeapon != null)
        {
            cards.Add(omnislash);
        }
        
        CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards.ToList(), base.Owner, canSkip: false);
        LimitManager.SetLimit(base.Owner, 0);
        PowerCmd.Remove<LimitBreakPower>(base.Owner.Creature);
        if (cardModel is Meteorain meteorain)
            await CardCmd.AutoPlay(choiceContext, cardModel, null);
        
        else await CardCmd.AutoPlay(choiceContext, cardModel, play.Target);
    }

    protected override void OnUpgrade()
    {
        
    }
}

    
    
