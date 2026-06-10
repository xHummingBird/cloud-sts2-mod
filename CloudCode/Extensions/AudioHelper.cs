using MegaCrit.Sts2.Core.Commands;

namespace Cloud.CloudCode.Extensions;


public static class AudioHelper
{
    private static readonly Random rng = new Random();

    private static readonly string[] battleStartSfx =
    {
        "res://Cloud/sounds/battle_start_1.wav",
        "res://Cloud/sounds/battle_start_2.wav",
    };
    
    private static readonly string[] attackSfx =
    {
        "res://Cloud/sounds/generic_attack_1.wav",
        "res://Cloud/sounds/generic_attack_2.wav",
        "res://Cloud/sounds/generic_attack_3.wav",
        "res://Cloud/sounds/generic_attack_4.wav",
        "res://Cloud/sounds/generic_attack_5.wav",
        "res://Cloud/sounds/generic_attack_6.wav"
    };
    
    private static readonly string[] magicSfx =
    {
        "res://Cloud/sounds/kurae",
        "res://Cloud/sounds/murata.wav",
        "res://Cloud/sounds/generic_magic_1.wav",
        "res://Cloud/sounds/generic_magic_2.wav",
        "res://Cloud/sounds/akiramero.wav"
    };
    
    private static readonly string[] damagedSfx =
    {
        "res://Cloud/sounds/damaged_1.wav",
        "res://Cloud/sounds/damaged_2.wav",
        "res://Cloud/sounds/damaged_3.wav",
        "res://Cloud/sounds/damaged_4.wav"
    };
    
    private static readonly string[] buffSfx =
    {
        "res://Cloud/sounds/kurae.wav",
        "res://Cloud/sounds/murata.wav",
        "res://Cloud/sounds/generic_magic_1.wav",
        "res://Cloud/sounds/generic_magic_2.wav",
        "res://Cloud/sounds/akiramero.wav"
    };
    
    private static readonly string[] highDamagedSfx =
    {
        "res://Cloud/sounds/damaged_high_1.wav",
        "res://Cloud/sounds/damaged_high_2.wav",
        "res://Cloud/sounds/damaged_high_3.wav",
        "res://Cloud/sounds/damaged_high_4.wav",
        "res://Cloud/sounds/damaged_high_5.wav"
    };
    
    private static readonly string[] criticalDamagedSfx =
    {
        "res://Cloud/sounds/damaged_critical_1.wav",
        "res://Cloud/sounds/damaged_critical_2.wav",
        "res://Cloud/sounds/damaged_critical_3.wav",
        "res://Cloud/sounds/damaged_critical_4.wav",
        "res://Cloud/sounds/damaged_critical_5.wav"
    };

    private static readonly string[] fireSfx =
    {
        "res://Cloud/sounds/fire_1.wav",
        "res://Cloud/sounds/fire_2.wav",
        "res://Cloud/sounds/fire_3.wav",
    };
    
    private static readonly string[] blizzardSfx =
    {
        "res://Cloud/sounds/blizzard.wav",
        "res://Cloud/sounds/blizzard_2.wav",
        "res://Cloud/sounds/blizzard_3.wav",
    };
    
    private static readonly string[] thunderSfx =
    {
        "res://Cloud/sounds/thunder_1.wav",
        "res://Cloud/sounds/thunder_2.wav",
        "res://Cloud/sounds/thunder_3.wav",
    };

    private static readonly string[] victorySfx =
    {
        "res://Cloud/sounds/not_interested.wav",
        "res://Cloud/sounds/victory_2.wav",
        "res://Cloud/sounds/victory_3.wav",
        "res://Cloud/sounds/victory_4.wav",
        "res://Cloud/sounds/victory_5.wav"
    };
    
    private static readonly string[] defendSfx =
    {
        "res://Cloud/sounds/generic_defend_1.wav",
        "res://Cloud/sounds/generic_defend_2.wav",
        "res://Cloud/sounds/generic_defend_3.wav",
        "res://Cloud/sounds/generic_defend_4.wav",
    };
    
    private static readonly string[] gameoverSfx =
    {
        "res://Cloud/sounds/gameover_ (1).wav",
        "res://Cloud/sounds/gameover_ (2).wav",
        "res://Cloud/sounds/gameover_ (3).wav",
        "res://Cloud/sounds/gameover_ (4).wav",
        "res://Cloud/sounds/gameover_ (5).wav"
    };
    
    public static void PlayRandomBattleStart()
    {
        PlayRandom(battleStartSfx);
    }
    
    public static void PlayRandomAttack()
    {
        PlayRandom(attackSfx);
    }
    
    public static void PlayRandomMagic()
    {
        PlayRandom(magicSfx);
    }
    
    public static void PlayRandomGameover()
    {
        PlayRandom(gameoverSfx);
    }
    
    public static void PlayRandomDamaged()
    {
        PlayRandom(damagedSfx);
    }
    
    public static void PlayRandomDamagedHigh()
    {
        PlayRandom(highDamagedSfx);
    }
    
    public static void PlayRandomDamagedCritical()
    {
        PlayRandom(criticalDamagedSfx);
    }
    
    public static void PlayRandomDefend()
    {
        PlayRandom(defendSfx);
    }
    
    public static void PlayRandomFire()
    {
        PlayRandom(fireSfx);
    }
    
    public static void PlayRandomBlizzard()
    {
        PlayRandom(blizzardSfx);
    }
    
    public static void PlayRandomThunder()
    {
        PlayRandom(thunderSfx);
    }

    public static void PlayRandomVictory()
    {
        PlayRandom(victorySfx);
    }

    public static void PlayRandom(string[] pool)
    {
        int index = rng.Next(pool.Length);
        SfxCmd.Play(pool[index]);
    }
}
