using System.Drawing;
using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Color = Godot.Color;

namespace Cloud.CloudCode.Cards.Ancient;

public class BraverKai() : CloudCard(1, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), IATBCard
{
    public int ATBCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15, ValueProp.Move),
        new PowerVar<VulnerablePower>(2)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var cloud = Owner?.Character as Character.Cloud;
        CinematicAttack.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && cloud != null)
        {
            SfxCmd.Play("res://Cloud/sfx/energy_charge.wav");
            SfxCmd.Play("res://Cloud/sounds/owaraseru.wav");
            SfxCmd.Play("res://Cloud/sfx/limit_break_thunder.wav");
            float duration1 = cloud.PlayAnimation(ownerCreature, "limit_break_1").total;
            if (duration1 > 0f)
                await Task.Delay((int)(duration1 * 1000f));
            await cloud.DashTo(ownerCreature, play.Target, distance: 300f);
            float duration = cloud.PlayAnimation(ownerCreature, "braver_kai").total;
            if (duration > 0f)
                await Task.Delay((int)(0.467f * 1000f));
            SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
            SfxCmd.Play("res://Cloud/sounds/braver.wav");
        }
        
        NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(play.Target);
        NBigSlashVfx nBigSlashVfx = NBigSlashVfx.Create(nCreature.GetBottomOfHitbox(), facingRight: true);
        cloud.DoScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_heavy_attack")
            .BeforeDamage(async delegate
                { NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nBigSlashVfx);
                    NBigSlashImpactVfx.Create(nCreature.GetBottomOfHitbox(), 180f, new Color("#80dbff"));}
            )
            .Execute(choiceContext);
        await Task.Delay((int)(0.63f * 1000f));
        if (ownerCreature != null && cloud != null)
        {
            await cloud.Retreat(ownerCreature);
        }
        if (!base.Owner.Creature.HasPower<PunisherModePower>())
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue,
                base.Owner.Creature, this);
        else await base.Owner.Creature.ExitPunisher();
        cloud?.RefreshIdle(ownerCreature);
        CinematicAttack.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}