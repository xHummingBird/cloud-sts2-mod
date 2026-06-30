using BaseLib.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using Cloud.CloudCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Cards.Ancient;

public class SummonCard() : CloudCard(0, CardType.Skill,
    CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => IsPlayable;
    protected override bool IsPlayable => base.Owner.HasPower<SummonPower>();
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
        bool hasOdin = base.Owner?.GetRelic<OdinMateria>() != null;
        bool hasBahamut = base.Owner?.GetRelic<BahamutMateria>() != null;
        
        var ifrit = CombatState.CreateCard<Ifrit>(base.Owner);
        var shiva = CombatState.CreateCard<Shiva>(base.Owner);
        var ramuh = CombatState.CreateCard<Ramuh>(base.Owner);
        var odin = CombatState.CreateCard<Odin>(base.Owner);
        var bahamut = CombatState.CreateCard<Bahamut>(base.Owner);
        
        List<CardModel> cards;
        
        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(ifrit);
            CardCmd.Upgrade(shiva);
            CardCmd.Upgrade(ramuh);
            CardCmd.Upgrade(odin);
            CardCmd.Upgrade(bahamut);

            cards = new()
            {
                ifrit,
                shiva,
                ramuh
            };
        }

        else
        {
            cards = new()
            {
                ifrit,
                shiva,
                ramuh
            };
        }
        
        if (hasOdin)
            cards.Add(odin);
        
        if (hasBahamut)
            cards.Add(bahamut);
        
        CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards.ToList(), base.Owner, canSkip: false);

        if (cardModel is Shiva shivaCard || cardModel is Ramuh ramuhCard)
        {
            SummonManager.SetSummon(base.Owner, 0);
            await PowerCmd.Remove<SummonPower>(base.Owner.Creature);
            await CardCmd.AutoPlay(choiceContext, cardModel, null);
        }
        else  if (cardModel is Ifrit ifritCard || cardModel is Odin odinCard)
        {
            SummonManager.SetSummon(base.Owner, 0);
            await PowerCmd.Remove<SummonPower>(base.Owner.Creature);
            await CardCmd.AutoPlay(choiceContext, cardModel, play.Target);
        }
        else if (cardModel is Bahamut bahamutCard)
            await CardCmd.AutoPlay(choiceContext, cardModel, base.Owner.Creature);
    }

    protected override void OnUpgrade()
    {
        
    }
}