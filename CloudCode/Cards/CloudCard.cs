using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Cloud.CloudCode.Character;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Cloud.CloudCode.Cards;

[Pool(typeof(CloudCardPool))]
public abstract class CloudCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    // Image size:
    // Normal art: 1000x760
    // Full art: 606x852
    public override string CustomPortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    public override string PortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public override string BetaPortraitPath =>
        $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override bool IsPlayable
    {
        get
        {
            // IMPORTANT:
            // Compendium / card library cards are canonical.
            // Do not call base.IsPlayable, Owner, ATBManager, or runtime logic on them.
            if (!IsMutable)
                return true;

            if (!base.IsPlayable)
                return false;

            if (this is not IATBCard)
                return true;

            int atbCost = ATBCostState.GetEffectiveATBCost(this);

            if (atbCost <= 0)
                return true;

            if (!TryGetOwner(out var owner) || owner == null)
                return true;

            return ATBManager.GetATB(owner) >= atbCost;
        }
    }

    private bool TryGetOwner(out Player? owner)
    {
        owner = null;

        if (!IsMutable)
            return false;

        try
        {
            owner = Owner;
            return owner != null;
        }
        catch
        {
            return false;
        }
    }
}