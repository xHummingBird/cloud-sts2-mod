using BaseLib.Extensions;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
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

public class Ifrit() : CloudCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), ISummonCard
{
    protected override bool ShouldGlowGoldInternal => IsPlayable;
    protected override bool IsPlayable => base.Owner.HasPower<SummonPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(30m, ValueProp.Move),
        new PowerVar<MagicResistDownPower>(3)
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
        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            float duration = cloud.PlayAnimation(ownerCreature, "ifrit").total;
            SfxCmd.Play("res://Cloud/sounds/summon_ifrit.wav");
            if (duration > 0f)
                await Task.Delay((int)(1.2f * 1000f));
        }
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
        if (nCreature != null)
        {
            NLargeMagicMissileVfx nLargeMagicMissileVfx = NLargeMagicMissileVfx.Create(nCreature.GetBottomOfHitbox(), new Color(Colors.Red));
            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nLargeMagicMissileVfx);
            await Cmd.Wait(nLargeMagicMissileVfx.WaitTime);
            NGame.Instance.ScreenShake(ShakeStrength.Medium, ShakeDuration.Short);
        }
        DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .BeforeDamage(async delegate
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(cardPlay.Target));
                SfxCmd.Play("event:/sfx/characters/attack_fire");
            })
            .Execute(choiceContext);
        PowerCmd.Apply<MagicResistDownPower>(choiceContext, cardPlay.Target, base.DynamicVars["MagicResistDownPower"].BaseValue, base.Owner.Creature, this);
        await Task.Delay((int)(1.9f * 1000f));
        CinematicAttack.End(RunManager.Instance.NetService.NetId);
    }
    protected override void OnUpgrade()
    {
        
    }
}