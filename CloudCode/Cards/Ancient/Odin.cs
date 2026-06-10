using BaseLib.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Odin() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), ISummonCard
{
    protected override bool ShouldGlowGoldInternal => IsPlayable;
    protected override bool IsPlayable => base.Owner.HasPower<SummonPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(32, ValueProp.Move),
        new DynamicVar("hpPercent", 15)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];
}