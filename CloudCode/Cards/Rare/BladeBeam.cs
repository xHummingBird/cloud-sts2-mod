using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Stance;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class BladeBeam() : CloudCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AllEnemies), IATBCard, IStanceCard
{
    public int ATBCost => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(18m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
        new PowerVar<VulnerablePower>(1m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        CloudStaticHoverTip.Stance
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            // attack animation
            float duration = cloud.PlayAnimation(ownerCreature, "blade_beam").total;
            SfxCmd.Play("res://Cloud/sounds/futobe.wav");
            SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
            SfxCmd.Play("res://Cloud/sfx/energy_charge.wav");
            // Optional: delay to sync hit roughly mid animation
            if (duration > 0f)
                await Task.Delay((int)(0.5f * 1000f));
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                NGame.Instance.ScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Blue);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                        SfxCmd.Play("event:/sfx/characters/attack_fire");
                    }
                }
            })
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext, base.CombatState.HittableEnemies, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, base.CombatState.HittableEnemies, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
        await Task.Delay((int)(0.3f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        await ownerCreature.TogglePunisher(1, base.Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
        DynamicVars.Weak.UpgradeValueBy(1m);
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}