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

namespace Cloud.CloudCode.Cards.Ancient;

public class HerosLastWish() : CloudCard(0, CardType.Skill,
    CardRarity.Ancient, TargetType.Self)
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
        new PowerVar<LimitBreakPower>(20),
        new PowerVar<SummonPower>(20)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/victory_2.wav");
            LimitManager.GainLimit(Owner, DynamicVars["LimitBreakPower"].IntValue);
            SummonManager.GainSummon(Owner, DynamicVars["SummonPower"].IntValue);
            ATBManager.GainATBDirect(ownerCreature.Player, 1);
        }
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
        DynamicVars["LimitBreakPower"].UpgradeValueBy(5);
        DynamicVars["SummonPower"].UpgradeValueBy(5);
    }
}