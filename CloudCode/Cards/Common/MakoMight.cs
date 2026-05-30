using BaseLib.Utils;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Cloud.CloudCode.Cards.Common;

public class MakoMight() : CloudCard(1, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<PunisherModePower>(1m),
        new PowerVar<VigorPower>(5m),
        new BlockVar(5, ValueProp.Move)
        
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PunisherModePower>(),
        HoverTipFactory.FromPower<VigorPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (!base.Owner.Creature.HasPower<PunisherModePower>())
        {
            if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            {
                SfxCmd.Play("res://Cloud/sounds/zenryokudeiku.wav");
                
                bool isPrime = ownerCreature.IsPrime();
                
                string shiftAnim = isPrime ? "mode_shift_2" : "mode_shift";
                string idleAnim  = isPrime ? "idle_prime_punisher" : "idle_punisher";
                
                float duration = cloud.PlayAnimation(ownerCreature, shiftAnim).total;
                
                if (duration > 0f)
                    await Task.Delay((int)(duration * 1000f));
                cloud.PlayAnimation(ownerCreature, idleAnim);
            }
        }

        if (!ownerCreature.IsPunisher())
        {
            PowerCmd.Apply<PunisherModePower>(choiceContext, base.Owner.Creature,
                base.DynamicVars["PunisherModePower"].BaseValue, base.Owner.Creature, this);
            await PowerCmd.Apply<VigorPower>(choiceContext, base.Owner.Creature,
                base.DynamicVars["VigorPower"].BaseValue, base.Owner.Creature, this);
        }
        else
        {
            AudioHelper.PlayRandomDefend();
            await CommonActions.CardBlock(this, cardPlay);
            await PowerCmd.Remove<PunisherModePower>(ownerCreature);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}