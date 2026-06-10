using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Common;

public class Ruin() : CloudCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IMagicCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(5m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
        new PowerVar<VulnerablePower>(1m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
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
            AudioHelper.PlayRandomMagic();
            // Optional: delay to sync hit roughly mid animation
            if (duration > 0f)
                await Task.Delay((int)(duration * 0.2f * 1000f));
        }
        await CommonActions.CardAttack(this, play.Target)
            .BeforeDamage(async delegate
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(play.Target, VfxColor.Black));
                SfxCmd.Play("event:/sfx/characters/attack_fire");
            })
            .Execute(choiceContext);
        if (play.Target != null)
        {
            if (Owner.Creature.IsPunisher())
            {
                await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue,
                    base.Owner.Creature, this);
            }
            else await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
        DynamicVars.Weak.UpgradeValueBy(1m);
    }
}