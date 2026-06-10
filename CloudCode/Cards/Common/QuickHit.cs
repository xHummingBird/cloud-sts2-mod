using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class QuickHit()  : CloudCard(0, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy), IATBCard
{
    public int ATBCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7m, ValueProp.Move),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/nameruna.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "quick_hit").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.133f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
            }
        }
        await CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (!base.Owner.Creature.IsPunisher())
        {
            ATBManager.GainATBDirect(base.Owner, 1);
        }
        await Task.Delay((int)(0.367f * 1000f));
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}