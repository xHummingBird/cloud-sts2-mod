using BaseLib.Abstracts;
using BaseLib.Utils;
using Cloud.CloudCode.Character;

namespace Cloud.CloudCode.Potions;

[Pool(typeof(CloudPotionPool))]
public abstract class CloudPotion : CustomPotionModel;