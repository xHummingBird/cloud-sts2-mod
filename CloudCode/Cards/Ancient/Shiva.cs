using BaseLib.Extensions;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class Shiva() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), ISummonCard
{
    protected override bool ShouldGlowGoldInternal => IsPlayable;
    // protected override bool IsPlayable => base.Owner.HasPower<SummonPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(17m, ValueProp.Move),
        new PowerVar<FrozenShieldPower>(3m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FrozenShieldPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CinematicAttack.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;
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
            await Task.Delay((int)(1.2f * 1000f));
        }
        DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
            .Execute(choiceContext);
        PowerCmd.Apply<FrozenShieldPower>(choiceContext, base.Owner.Creature, base.DynamicVars["FrozenShieldPower"].BaseValue, base.Owner.Creature, this);
        await Task.Delay((int)(1.0f * 1000f));
        CinematicAttack.End(RunManager.Instance.NetService.NetId);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];
}