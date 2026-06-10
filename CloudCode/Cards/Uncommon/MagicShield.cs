using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class MagicShield() : CloudCard(0, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self), IATBCard
{
    public int ATBCost => 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<MagicShieldPower>(4m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            // attack animation
            float duration = cloud.PlayAnimation(ownerCreature, "cast").total;
            AudioHelper.PlayRandomDefend();
            // Optional: delay to sync hit roughly mid animation
            if (duration > 0f)
                await Task.Delay((int)(duration * 0.2f * 1000f));
        }
        await PowerCmd.Apply<MagicShieldPower>(choiceContext, base.Owner.Creature, base.DynamicVars["MagicShieldPower"].BaseValue, base.Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        base.DynamicVars["MagicShieldPower"].UpgradeValueBy(2m);
    }
}