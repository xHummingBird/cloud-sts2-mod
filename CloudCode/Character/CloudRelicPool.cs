using BaseLib.Abstracts;
using Cloud.CloudCode.Extensions;
using Godot;

namespace Cloud.CloudCode.Character;

public class CloudRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Cloud.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}