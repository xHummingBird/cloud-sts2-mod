using BaseLib.Extensions;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Shiva() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), ISummonCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(25m, ValueProp.Move),
        new PowerVar<FrozenShieldPower>(3m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        CloudStaticHoverTip.Magic,
        HoverTipFactory.FromPower<FrozenShieldPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;
        PowerCmd.Remove<SummonPower>(base.Owner.Creature);
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            float duration = cloud.PlayAnimation(ownerCreature, "shiva").total;
            SfxCmd.Play("res://Cloud/sounds/summon_shiva.wav");
            var targets = base.CombatState.HittableEnemies;
            if (duration > 0f)
                await Task.Delay((int)(0.9f * 1000f));
            foreach (var target in targets)
            {
                cloud.PlayVfxOnTarget(
                    target,
                    "res://Cloud/scenes/ice_vfx.tscn",
                    "diamond_dust"
                );
            }
            await Task.Delay((int)(0.2333f * 1000f));
            SfxCmd.Play("res://Cloud/sfx/ice.wav");
            await Task.Delay((int)(0.7f * 1000f));
            SfxCmd.Play("res://Cloud/sfx/ice_2.wav");
        }
        DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                NGame.Instance.ScreenShake(ShakeStrength.TooMuch, ShakeDuration.Normal);
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Blue);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    }
                }
            })
            .Execute(choiceContext);
        await Task.Delay((int)(1.1f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        await PowerCmd.Apply<FrozenShieldPower>(choiceContext, base.Owner.Creature, base.DynamicVars["FrozenShieldPower"].BaseValue, base.Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10);
    }
}