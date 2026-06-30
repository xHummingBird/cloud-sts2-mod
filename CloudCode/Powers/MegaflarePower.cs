using BaseLib.Utils;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class MegaflarePower : CloudPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(70m, ValueProp.Move)
    ];

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext playerChoiceContext, Player player)
    {
        if (player != base.Owner.Player)
            return;
        if (Amount < 3)
        {
            await PowerCmd.Apply<MegaflarePower>(new ThrowingPlayerChoiceContext(), base.Owner, 1, null, null);
            return;
        }
        
        if (Amount >= 3)
        {
            SummonManager.SetSummon(base.Owner.Player, 0);
            await PowerCmd.Remove<SummonPower>(base.Owner);
            var ownerCreature = Owner.Player?.Creature;
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);  
            if (ownerCreature != null && Owner.Player?.Character is Character.Cloud cloud)
            {
                // attack animation
                float duration = cloud.PlayAnimation(ownerCreature, "bahamut").total;
                SfxCmd.Play("res://Cloud/sounds/summon_bahamut.wav");
                await Task.Delay((int)(1.0f * 1000f));
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
                NLargeMagicMissileVfx? vfx = NLargeMagicMissileVfx.Create(center, new Color(Colors.Purple));
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                await Cmd.Wait(vfx.WaitTime);
                
                NGame.Instance.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx2 = NGroundFireVfx.Create(target, VfxColor.Purple);
                    if (vfx2 != null)
                    {
                        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx2);
                        SfxCmd.Play("event:/sfx/characters/attack_fire");
                        SfxCmd.Play("blunt_attack.mp3");
                        VfxCmd.PlayOnCreatureCenter(
                            target,
                            VfxCmd.bluntPath
                        );

                    }
                }
                await CreatureCmd.Damage(playerChoiceContext, targets, DynamicVars.Damage.BaseValue, ValueProp.Move, base.Owner);
                await PowerCmd.Remove<MegaflarePower>(base.Owner);
                await Task.Delay((int)(1.8f * 1000f));
                CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            }
        }
    }
}