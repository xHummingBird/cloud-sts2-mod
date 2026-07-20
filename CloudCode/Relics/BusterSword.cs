using MegaCrit.Sts2.Core.Entities.Relics;

namespace Cloud.CloudCode.Relics;

public class BusterSword : LimitRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override int BaseTurnLimitGain => 3;
    protected override int FuryTurnLimitGain => 6;

    protected override int BaseTurnSummonGain => 3;
    protected override int SummonUpTurnSummonGain => 6;

    protected override int AttackSummonGain => 3;
    protected override int MagicSummonGain => 6;
}