using BaseLib.Extensions;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Odin() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), ISummonCard
{
    protected override bool ShouldGlowGoldInternal => IsPlayable;
    protected override bool IsPlayable => base.Owner.HasPower<SummonPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(32, ValueProp.Move),
        new DynamicVar("hpPercent", 15),
        new PowerVar<VulnerablePower>(3)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var cloud = Owner?.Character as Character.Cloud;
        decimal threshold = DynamicVars["hpPercent"].BaseValue;
        
        CinematicAttack.Start(RunManager.Instance.NetService.NetId);
        PowerCmd.Remove<SummonPower>(base.Owner.Creature);
        if (ownerCreature != null && cloud != null)
        {
            SfxCmd.Play("res://Cloud/sounds/summon_odin.wav");
            cloud.PlayAnimation(ownerCreature, "cast_operator");
            cloud.PlayVfxOnTarget(
                play.Target,
                "res://Cloud/scenes/vfx.tscn",
                "odin_vfx"
            );
                await Task.Delay((int)(1.25f * 1000f));
                NGame.Instance.ScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
                    .Execute(choiceContext);
                await Task.Delay((int)(1.55 * 1000f));
                // cam?.EndCinematic();
                CinematicAttack.End(RunManager.Instance.NetService.NetId);
        }
        else
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
                .Execute(choiceContext);
        }
        
        if (play.Target.CurrentHp * 100 <= play.Target.MaxHp * threshold && play.Target.CurrentHp > 0)
        {
            await DoomPower.DoomKill(new List<Creature> { play.Target });
            return;
        }
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);  
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars["hpPercent"].UpgradeValueBy(5m);
    }
}