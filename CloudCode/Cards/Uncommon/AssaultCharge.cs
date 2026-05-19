using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class AssaultCharge()  : CloudCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14m, ValueProp.Move),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/kimeru.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "quick_hit").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.133f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
            }
        }
        await CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (base.Owner.Creature.IsPunisher())
        {
            await PowerCmd.Apply<FreeAttackPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        }
        else await base.Owner.Creature.EnterPunisher(1, base.Owner.Creature, this);
            
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}