using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class JudgmentBoltPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7m, ValueProp.Move)
    ];

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext playerChoiceContext, Player player)
    {
        if (player != base.Owner.Player)
            return;
        Creature creature = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(base.Owner.CombatState.HittableEnemies);
        if (creature != null)
        {
            VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_attack_lightning");
            SfxCmd.Play("event:/sfx/characters/defect/defect_lightning_passive");
            await CreatureCmd.Damage(playerChoiceContext, creature, DynamicVars.Damage.BaseValue, ValueProp.Unpowered, base.Owner);
        }
        await PowerCmd.Decrement(this);
    }
}