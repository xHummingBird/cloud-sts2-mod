using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Cloud.CloudCode.Character;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Cloud.CloudCode.Cards;

[Pool(typeof(CloudCardPool))]
public abstract class CloudCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    
    protected override bool IsPlayable
    {
        get
        {
            // Preserve any other "card logic" gating upstream
            if (!base.IsPlayable)
                return false;

            // Only ATB-cost cards are gated
            if (this is not IATBCard atbCard)
                return true;

            // Owner getter in CardModel asserts mutability in the decompile you posted,
            // so only read Owner when mutable. (Combat cards are mutable.)
            if (!IsMutable)
                return true;

            // If you want ATB cost 0 to behave like "no gate"
            int cost = atbCard.ATBCost;
            if (cost <= 0)
                return true;

            return ATBManager.GetATB(Owner) >= cost;
        }
    }

}