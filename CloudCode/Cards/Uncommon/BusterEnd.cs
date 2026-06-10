using BaseLib.Utils;
using Cloud.CloudCode.Mechanics.ATB;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class BusterEnd() : CloudCard(3, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy), IATBCard
{
    public int ATBCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15m, ValueProp.Move),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            SfxCmd.Play("res://Cloud/sounds/shizume.wav");
            float duration = cloud.PlayAnimation(ownerCreature, "braver").total;
            if (duration > 0f)
            {
                await Task.Delay((int)(0.2f * 1000f));
                SfxCmd.Play("res://Cloud/sfx/sword_swing.wav");
                await Task.Delay((int)(0.45f * 1000f));
            }
        }
        CommonActions.CardAttack(this, play.Target).WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await Task.Delay((int)(0.567f * 1000f));
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
    
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this)
        {
            return Task.CompletedTask;
        }
        if (base.IsClone)
        {
            return Task.CompletedTask;
        }
        int amount = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Card.Owner == base.Owner && e.HappenedThisTurn(base.CombatState));
        ReduceCostBy(amount);
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner)
        {
            return Task.CompletedTask;
        }
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return Task.CompletedTask;
        }
        ReduceCostBy(1);
        return Task.CompletedTask;
    }

    private void ReduceCostBy(int amount)
    {
        base.EnergyCost.AddThisTurn(-amount);
    }
}