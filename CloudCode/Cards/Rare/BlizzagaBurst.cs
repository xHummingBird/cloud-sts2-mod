using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Summon;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Rare;

public class BlizzagaBurst() : CloudCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AllEnemies), IATBCard, IMagicCard
{
    public int ATBCost => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(15m, ValueProp.Move),
        new BlockVar(10, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        
        var enemies = CombatState.Enemies;
        bool shouldTriggerFatal = enemies
            .Where(e => !e.IsDead)
            .All(enemy =>
                enemy.Powers.All(p => p.ShouldOwnerDeathTriggerFatal()));

        if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
        {
            // attack animation
            float duration = cloud.PlayAnimation(ownerCreature, "cast").total;
            AudioHelper.PlayRandomBlizzard();
            // Optional: delay to sync hit roughly mid animation
            if (duration > 0f)
                await Task.Delay((int)(duration * 0.2f * 1000f));
        }
        
        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Blue);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                        SfxCmd.Play("res://Cloud/sfx/ice.wav");
                    }
                }
            })
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, play);
        if (shouldTriggerFatal && attackCommand.Results.SelectMany((List<DamageResult> r) => r)
                .Any((DamageResult r) => r.WasTargetKilled))
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}