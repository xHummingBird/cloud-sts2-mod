using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Ancient;

public class LimitBreak() : CloudCard(1, CardType.Skill,
    CardRarity.Ancient, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(18, ValueProp.Move),
        new PowerVar<VulnerablePower>(2)
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

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sfx/energy_charge.wav");
            SfxCmd.Play("res://Cloud/sounds/limit_break.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "braver").total;
            if (duration > 0f)
                await Task.Delay((int)(1.9f * 1000f));
            SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
            SfxCmd.Play("res://Cloud/sounds/braver.wav");
            
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (!base.Owner.Creature.HasPower<PunisherModePower>())
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue,
                base.Owner.Creature, this);
        else await base.Owner.Creature.ExitPunisher();
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(7);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}