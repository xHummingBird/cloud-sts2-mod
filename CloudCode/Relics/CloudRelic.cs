using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Cloud.CloudCode.Character;
using Cloud.CloudCode.Extensions;
using Godot;

namespace Cloud.CloudCode.Relics;

[Pool(typeof(CloudRelicPool))]
public abstract class CloudRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".BigRelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}