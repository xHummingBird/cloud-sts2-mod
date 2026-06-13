using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Zantetsuken() : CloudCard(2, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), IATBCard
{
    public int ATBCost => 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(22, ValueProp.Move),
        new DynamicVar("hpPercent", 10)
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var cloud = Owner?.Character as Character.Cloud;
        decimal threshold = DynamicVars["hpPercent"].BaseValue;
        
        if (ownerCreature != null && cloud != null)
        {
            CinematicAttack.Start(RunManager.Instance.NetService.NetId);
            SfxCmd.Play("res://Cloud/sounds/zantetsuken.wav");
            await cloud.DashTo(ownerCreature, play.Target, distance: 300f);
            cloud.PlayVfxOnTarget(
                play.Target,
                "res://Cloud/scenes/vfx.tscn",
                "zantetsuken_vfx"
            );
            float duration = cloud.PlayAnimation(ownerCreature, "zantetsuken").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.8f * 1000f));
                cloud.DashPast(ownerCreature, play.Target, null, 0.01f, 300f);
                NGame.Instance.ScreenShake(ShakeStrength.TooMuch, ShakeDuration.Short);
                DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
                    .Execute(choiceContext);
                await Task.Delay((int)(0.6 * 1000f));
                if (play.Target != null) {
                    if (play.Target.CurrentHp * 100 <= play.Target.MaxHp * threshold)
                        await CreatureCmd.Kill(play.Target);
                }
                await cloud.Retreat(ownerCreature);
            }
            CinematicAttack.End(RunManager.Instance.NetService.NetId);
        }
        else
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
                .Execute(choiceContext);
            if (play.Target.CurrentHp * 100 <= play.Target.MaxHp * threshold)
                DamageCmd.Attack(play.Target.CurrentHp).FromCard(this).Targeting(play.Target)
                    .Execute(choiceContext);
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars["hpPercent"].UpgradeValueBy(5m);
    }
}
    

    
