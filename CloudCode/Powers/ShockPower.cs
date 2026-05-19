using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Powers;

public class ShockPower : CloudPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side != base.Owner.Side)
        {
            return;
        }
        VfxCmd.PlayOnCreature(base.Owner, "vfx/vfx_attack_lightning");
        SfxCmd.Play("event:/sfx/characters/defect/defect_lightning_evoke");
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        if (base.Owner.IsAlive)
        {
            await PowerCmd.Decrement(this);
        }
        else
        {
            await Cmd.CustomScaledWait(0.1f, 0.25f);
        }
    }
}