using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Cards.Uncommon;

public class ChangeTheTempo()  : CloudCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.AnyEnemy), IATBCard
{
    public int ATBCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<PunisherModePower>(1m),
        new PowerVar<VulnerablePower>(2m),
        new PowerVar<WeakPower>(2m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PunisherModePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (!base.Owner.Creature.HasPower<PunisherModePower>())
        {
            await PowerCmd.Apply<PunisherModePower>(choiceContext, base.Owner.Creature, base.DynamicVars["PunisherModePower"].BaseValue, base.Owner.Creature, this);
            if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            {
                SfxCmd.Play("res://Cloud/sounds/zenryokudeiku.wav");
                float duration = cloud.PlayAnimation(ownerCreature, "mode_shift").total;
                if (duration > 0f)
                    await Task.Delay((int)(duration * 0.9f * 1000f));
                cloud.PlayAnimation(ownerCreature, "idle_punisher");
            }
            await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Remove<PunisherModePower>(base.Owner.Creature);
            if (Owner?.Character is Character.Cloud cloud)
            {
                AudioHelper.PlayRandomDefend();
                cloud.RefreshIdle(ownerCreature);
            }
            await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}