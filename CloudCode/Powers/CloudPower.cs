using BaseLib.Abstracts;
using BaseLib.Extensions;
using Cloud.CloudCode.Extensions;
using Godot;

namespace Cloud.CloudCode.Powers;

public abstract class CloudPower : CustomPowerModel
{
    //Loads from Cloud/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}