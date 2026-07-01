using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Stance;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class BusterBrace() : CloudCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self), IStanceCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        CloudStaticHoverTip.Stance,
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);
        var ownerCreature = Owner?.Creature;
        await ownerCreature.TogglePunisher(1, ownerCreature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(3m);
    }
}