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

public class Ramuh() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), ISummonCard
{
    protected override bool ShouldGlowGoldInternal => IsPlayable;
    protected override bool IsPlayable => base.Owner.HasPower<SummonPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(17m, ValueProp.Move),
        new PowerVar<JudgmentBoltPower>(3m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<JudgmentBoltPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CinematicAttack.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;
        PowerCmd.Remove<SummonPower>(base.Owner.Creature);
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            float duration = cloud.PlayAnimation(ownerCreature, "ramuh").total;
            SfxCmd.Play("res://Cloud/sounds/summon_ramuh.wav");
            if (duration > 0f)
                await Task.Delay((int)(1.2f * 1000f));
        }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_attack_lightning", "event:/sfx/characters/defect/defect_lightning_passive")
            .Execute(choiceContext);
        await Task.Delay((int)(0.9f * 1000f));
        CinematicAttack.End(RunManager.Instance.NetService.NetId);
        await PowerCmd.Apply<JudgmentBoltPower>(choiceContext, base.Owner.Creature, base.DynamicVars["JudgmentBoltPower"].BaseValue, base.Owner.Creature, this);
        await Task.Delay((int)(0.5f * 1000f));
    }
}