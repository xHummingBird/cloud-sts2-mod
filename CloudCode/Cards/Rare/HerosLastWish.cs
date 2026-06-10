using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Cloud.CloudCode.Cards.Rare;

public class HerosLastWish() : CloudCard(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SummonPower>(),
        HoverTipFactory.FromPower<LimitBreakPower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new EnergyVar(1),
        new PowerVar<LimitBreakPower>(15),
        new PowerVar<SummonPower>(15)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/victory_2.wav");
            LimitManager.GainLimit(Owner, DynamicVars["LimitBreakPower"].IntValue);
            await Owner.Creature.CheckLimitReady(
                choiceContext,
                Owner.Creature,
                null
            );
            SummonManager.GainSummon(Owner, DynamicVars["SummonPower"].IntValue);
            await Owner.Creature.CheckSummonReady(
                choiceContext,
                Owner.Creature,
                null
            );
            ATBManager.GainATBDirect(ownerCreature.Player, 1);
        }
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
    }
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}