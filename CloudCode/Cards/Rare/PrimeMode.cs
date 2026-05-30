using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Cloud.CloudCode.Cards.Rare;

public class PrimeMode() : CloudCard(2, CardType.Power,
    CardRarity.Rare, TargetType.Self), IATBCard
    {
        public int ATBCost => 1;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new PowerVar<PrimeModePower>(1m),
        ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<PrimeModePower>(),
            HoverTipFactory.FromPower<PunisherModePower>()
        ];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var ownerCreature = Owner?.Creature;
            if (ownerCreature != null && Owner?.Character is Character.Cloud cloud)
            {
                SfxCmd.Play("res://Cloud/sounds/chikara_wo_misetearu.wav");
                SfxCmd.Play("res://Cloud/sfx/limit_break_thunder.wav");
                bool isPunisher = ownerCreature.IsPunisher();

                string shiftAnim = isPunisher ? "limit_break_1" : "limit_break_2";
                string idleAnim = isPunisher ? "idle_prime_punisher" : "idle_prime_operator";

                float duration = cloud.PlayAnimation(ownerCreature, shiftAnim).total;

                if (duration > 0f)
                {
                    await Task.Delay((int)(duration * 1000f));
                    cloud.PlayAnimation(ownerCreature, idleAnim);
                }
            }

            PowerCmd.Apply<PrimeModePower>(choiceContext, base.Owner.Creature, base.DynamicVars["PrimeModePower"].BaseValue,
                base.Owner.Creature, this);
        }
        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }

    
