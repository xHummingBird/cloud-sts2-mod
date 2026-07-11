using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Stance;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class Omnibreak() : CloudCard(1, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IStanceCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new PowerVar<WeakPower>(1),
        new PowerVar<VulnerablePower>(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        CloudStaticHoverTip.Stance
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            
            AudioHelper.PlayRandomAttack();
            float duration = cloud.PlayAnimation(ownerCreature, "attack").total;
            if (duration > 0f)
                await Task.Delay((int)(0.13f * 1000f));
            SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
            await Task.Delay((int)(0.14f * 1000f));
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx("vfx/vfx_attack_slash", "res://Cloud/sfx/cloud_hit.wav")
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, base.DynamicVars.Weak.BaseValue,
            base.Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue,
            base.Owner.Creature, this);
        await ownerCreature.TogglePunisher(1, ownerCreature, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        AddKeyword(CardKeyword.Innate);
    }
}