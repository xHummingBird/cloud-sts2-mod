using BaseLib.Utils;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class WindUpSlash() : CloudCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12m, ValueProp.Move),
        new PowerVar<VigorPower>(5m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/futobe.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "wind_up_slash").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.433f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
            }
            await CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        decimal bonusVigor = DynamicVars["VigorPower"].BaseValue + 2;
        if (base.Owner.Creature.HasPower<PunisherModePower>())
            await PowerCmd.Apply<VigorPower>(choiceContext, base.Owner.Creature, bonusVigor, base.Owner.Creature, this);
        else await PowerCmd.Apply<VigorPower>(choiceContext, base.Owner.Creature, base.DynamicVars["VigorPower"].BaseValue, base.Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}