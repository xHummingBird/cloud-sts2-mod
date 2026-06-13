using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Cloud.CloudCode.Powers;

public class SummonPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var tips = new List<IHoverTip>();

            // === Default summons ===
            tips.Add(HoverTipFactory.FromCard<Ifrit>());
            tips.Add(HoverTipFactory.FromCard<Shiva>());
            tips.Add(HoverTipFactory.FromCard<Ramuh>());

            // === Check materia ===
            bool hasOdin = base.Owner.Player?.GetRelic<OdinMateria>() != null;
            bool hasBahamut = base.Owner.Player?.GetRelic<BahamutMateria>() != null;

            if (hasOdin)
            {
                tips.Add(HoverTipFactory.FromCard<Odin>());
            }

            if (hasBahamut)
            {
                tips.Add(HoverTipFactory.FromCard<Bahamut>());
            }
            return tips;
        }
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SfxCmd.Play("res://Cloud/sounds/summon_choose.wav");
        
        List<CardModel> cards = new();

        bool hasOdin = base.Owner.Player?.GetRelic<OdinMateria>() != null;
        bool hasBahamut = base.Owner.Player?.GetRelic<BahamutMateria>() != null;
        
        cards.Add(CombatState.CreateCard<Ifrit>(base.Owner.Player));
        cards.Add(CombatState.CreateCard<Shiva>(base.Owner.Player));
        cards.Add(CombatState.CreateCard<Ramuh>(base.Owner.Player));

        if (hasOdin)
        {
            cards.Add(CombatState.CreateCard<Odin>(base.Owner.Player));
        }

        if (hasBahamut)
        {
            cards.Add(CombatState.CreateCard<Bahamut>(base.Owner.Player));
        }
        
        CardModel? cardModel = await CardSelectCmd.FromChooseACardScreen(new ThrowingPlayerChoiceContext(), cards.ToList(), base.Owner.Player, canSkip: false);
        await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, base.Owner.Player);

        Flash();
    }
}