using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Stance;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class Climhazzard() : CloudCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IATBCard, IPunisherCard
{
    public int ATBCost => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(21, ValueProp.Move),
        new PowerVar<ArmorBreakPower>(50m),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        CloudStaticHoverTip.Punisher
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            await cloud.DashTo(ownerCreature, play.Target, distance: 550f);
            float duration = cloud.PlayAnimation(ownerCreature, "climhazzard").total;
            if (duration > 0f)
            {
                SfxCmd.Play("res://Cloud/sounds/sokoda.wav");
                await Task.Delay((int)(0.1f * 1000f));
                CloudExtensions.CombatHelpers.FakeHit(play.Target);
                
                await Task.Delay((int)(0.3f * 1000f));
                CloudExtensions.CombatHelpers.FakeHit(play.Target);
                
                await Task.Delay((int)(0.65f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing_heavy.wav");
                SfxCmd.Play("res://Cloud/sounds/kochida.wav");
                
                CommonActions.CardAttack(this, play.Target)
                    .WithHitFx("vfx/vfx_attack_slash",
                        "res://Cloud/sfx/omnislash_finalhit.wav")
                    .Execute(choiceContext);
                SfxCmd.Play("res://Cloud/sfx/cloud_hit.wav");
                cloud.DoScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
                
                await Task.Delay((int)(0.70f * 1000f));
                await cloud.Retreat(ownerCreature);
                await PowerCmd.Apply<ArmorBreakPower>(choiceContext, play.Target,
                    base.DynamicVars["ArmorBreakPower"].BaseValue,
                    base.Owner.Creature, this);
            }
        }
        else
        {
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<ArmorBreakPower>(choiceContext, play.Target,
                    base.DynamicVars["ArmorBreakPower"].BaseValue,
                    base.Owner.Creature, this);
        }
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        await ownerCreature.EnterPunisher(1, base.Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        base.DynamicVars["ArmorBreakPower"].UpgradeValueBy(20);
    }
}