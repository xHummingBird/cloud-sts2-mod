using MegaCrit.Sts2.Core.Entities.Powers;

namespace Cloud.CloudCode.Powers;

public class FuryPower : CloudPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}