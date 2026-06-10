using Cloud.CloudCode.Cards;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Uncommon;

public class Counterstance() : CloudCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public int ATBCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [
            new BlockVar(15m, ValueProp.Move),
            new PowerVar<CounterStancePower>(1m),
            new PowerVar<PunisherModePower>(1)
        ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<CounterStancePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (!base.Owner.Creature.HasPower<PunisherModePower>())
        {
            if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            {
                SfxCmd.Play("res://Cloud/sounds/kakatekoi.wav");
                float duration = cloud.PlayAnimation(ownerCreature, "mode_shift").total;
                if (duration > 0f)
                    await Task.Delay((int)(duration * 0.9f * 1000f));
                cloud.PlayAnimation(ownerCreature, "idle_punisher");
            }
            await PowerCmd.Apply<PunisherModePower>(choiceContext, base.Owner.Creature, base.DynamicVars["PunisherModePower"].BaseValue, base.Owner.Creature, this);
        }
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<CounterStancePower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5m);
    }
}