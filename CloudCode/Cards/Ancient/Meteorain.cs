using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Limit;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Meteorain() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), ILimitCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(25m, ValueProp.Move),
        new PowerVar<WeakPower>(2m),
        new PowerVar<VulnerablePower>(2m)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {

        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            // attack animation
            float duration = cloud.PlayAnimation(ownerCreature, "cast_punisher").total;
            SfxCmd.Play("res://Cloud/sounds/meteorain.wav");
            // Optional: delay to sync hit roughly mid animation
            if (duration > 0f)
                await Task.Delay((int)(duration * 0.9f * 1000f));
            cloud.PlayAnimation(ownerCreature, "cast_operator");
        }
        {
            var enemies = base.CombatState.HittableEnemies.ToList();
            if (enemies.Count == 0)
                return;
            Vector2 center = Vector2.Zero;
            int count = 0;
            foreach (var enemy in enemies)
            {
                var node = NCombatRoom.Instance?.GetCreatureNode(enemy);
                if (node != null)
                {
                    center += node.GetBottomOfHitbox();
                    count++;
                }
            }
            if (count == 0)
                return;
            center /= count;
            NLargeMagicMissileVfx vfx = NLargeMagicMissileVfx.Create(center, new Color("50b598"));
            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
            await Cmd.Wait(vfx.WaitTime);
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;

                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Red);
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
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
        DynamicVars.Weak.UpgradeValueBy(1m);
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}