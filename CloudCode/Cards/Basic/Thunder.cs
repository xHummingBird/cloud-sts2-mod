using BaseLib.Utils;
using Cloud.CloudCode.Cards;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Basic;

public class Thunder() : CloudCard(1, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(5m, ValueProp.Move),
        new PowerVar<ShockPower>(3m),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            // attack animation
            float duration = cloud.PlayAnimation(ownerCreature, "cast").total;
            AudioHelper.PlayRandomThunder();
            // Optional: delay to sync hit roughly mid animation
            if (duration > 0f)
                await Task.Delay((int)(duration * 0.2f * 1000f));
        }
        
        await CommonActions.CardAttack(this, play.Target)
            .BeforeDamage(async delegate
            {
                VfxCmd.PlayOnCreature(play.Target, "vfx/vfx_attack_lightning");
                SfxCmd.Play("event:/sfx/characters/defect/defect_lightning_evoke");
            })
            .Execute(choiceContext);
        
        await PowerCmd.Apply<ShockPower>(choiceContext, play.Target, base.DynamicVars["ShockPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        base.DynamicVars["ShockPower"].UpgradeValueBy(2m);
    }
}