using Cloud.CloudCode.Cards.Ancient;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;

namespace Cloud.CloudCode.Relics;

public class ChampionBelt() : CloudRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<LimitBreak>(true),
    ];
    
}