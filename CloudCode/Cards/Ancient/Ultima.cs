using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Summon;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Ultima() : CloudCard(2, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), IMagicCard, IATBCard
{
    public int ATBCost => 3;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(27m, ValueProp.Move),
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
            SfxCmd.Play("res://Cloud/sounds/ultima.wav");
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
            NLargeMagicMissileVfx vfx = NLargeMagicMissileVfx.Create(center, new Color(Colors.Blue));
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
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Blue);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                        SfxCmd.Play("event:/sfx/characters/attack_fire");
                    }
                }
            })
            .Execute(choiceContext);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}