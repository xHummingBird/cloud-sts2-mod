using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Stance;
using Cloud.CloudCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Basic;

public class Braver() : CloudCard(1, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy), IATBCard, IOperatorCard
{
    public int ATBCost => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new PowerVar<VulnerablePower>(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        CloudStaticHoverTip.Operator,
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var cloud = Owner?.Character as Character.Cloud;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && cloud != null)
        {
            
            SfxCmd.Play("res://Cloud/sounds/owaraseru.wav");
            await cloud.DashTo(ownerCreature, play.Target, distance: 300f);
            float duration = cloud.PlayAnimation(ownerCreature, "braver_kai").total;
            if (duration > 0f)
                await Task.Delay((int)(0.467f * 1000f));
            SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
            SfxCmd.Play("res://Cloud/sounds/braver.wav");
            
        }
        
        NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(play.Target);
        NBigSlashVfx nBigSlashVfx = NBigSlashVfx.Create(nCreature.GetBottomOfHitbox(), facingRight: true);
        
        CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Cloud/sfx/cloud_hit.wav")
            .BeforeDamage(async delegate
                { NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nBigSlashVfx);
                    NBigSlashImpactVfx.Create(nCreature.GetBottomOfHitbox(), 180f, new Color("#80dbff"));
                }
                )
            .Execute(choiceContext);
        cloud.DoScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
        await Task.Delay((int)(0.63f * 1000f));
        if (ownerCreature != null && cloud != null)
        {
            await cloud.Retreat(ownerCreature);
        }
        if (!base.Owner.Creature.HasPower<PunisherModePower>())
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue,
                base.Owner.Creature, this);
        else await base.Owner.Creature.ExitPunisher();
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}