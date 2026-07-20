using MegaCrit.Sts2.Core.Entities.Relics;

namespace Cloud.CloudCode.Relics;

public class UltimaWeapon : LimitRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override int BaseTurnLimitGain => 5;
    protected override int FuryTurnLimitGain => 8;

    protected override int BaseTurnSummonGain => 5;
    protected override int SummonUpTurnSummonGain => 10;
    
    protected override int AttackSummonGain => 3;

    protected override int MagicSummonGain => 6;
}