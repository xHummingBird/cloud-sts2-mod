using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Stance;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class Disorder() : CloudCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy), IATBCard, IStanceCard
{
    public int ATBCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10m, ValueProp.Move),
        new BlockVar(5m, ValueProp.Move),
        new EnergyVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        CloudStaticHoverTip.Stance
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay? play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/shizume.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "braver").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.2f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                await Task.Delay((int)(0.45f * 1000f));
            }
        }
        if (base.Owner.Creature.IsPunisher())
        {
           await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play).WithHitCount(2).Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Cloud/sfx/cloud_hit.wav")
                .Execute(choiceContext);
                AudioHelper.PlayRandomDefend();
                await CommonActions.CardBlock(this, play);
            await ownerCreature.ExitPunisher();
        }
        else if (!base.Owner.Creature.IsPunisher())
        {
            CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash", "res://Cloud/sfx/cloud_hit.wav")
                .Execute(choiceContext);
            await PowerCmd.Apply<FreeAttackPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature,
                    this);
            await base.Owner.Creature.EnterPunisher(1, base.Owner.Creature, this);
        }
        await Task.Delay((int)(0.567f * 1000f));
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}