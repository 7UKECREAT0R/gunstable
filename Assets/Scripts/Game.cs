using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// All the game-specific stuff that doesn't really need to be instanced in the scene.
/// </summary>
public static class Game
{
    /// <summary>
    /// Returns true if a random roll should be a gun, false if the roll should be an item.
    /// </summary>
    /// <returns>
    /// Returns a boolean value indicating whether the random roll should be a gun.
    /// </returns>
    public static bool RollDropType()
    {
        const float ITEM_PERCENT = 5;
        return Random.value < (100F - ITEM_PERCENT) / 100F;
    }
    public static LuckyObject RollLuckyObject()
    {
        int pick = Random.Range(0, 5);
        return (LuckyObject)pick;
    }
    public static Gun RollGun()
    {
        int gun = Random.Range(0, Guns.ALL_GUNS.Length);
        Gun roll = Guns.ALL_GUNS[gun];
        roll.rarity = Rarity.Roll(RarityRolls);
        return roll;
    }

    public const float baseInterestTime = 7.5F;
    public const float defaultPickupRange = 0.2F;
    public const float bulletTimeSlowness = 0.3F;
    
    private static int luckyClovers;
    private static int luckyRocks;
    private static int luckySprings;
    private static int luckySandbags;
    private static int luckyRings;
    
    /// <summary>
    /// Number of rarity rolls the player gets.
    /// </summary>
    public static int RarityRolls => 1 + luckyClovers;

    /// <summary>
    /// Multiplier to be combined with the thrown weapon damage.
    /// </summary>
    public static float ThrownWeaponMultiplier => luckyRocks > 0 ? 1.5F * luckyRocks : 1.0F;
    /// <summary>
    /// The distance the player can click to pickup/dash towards items/guns on the ground.
    /// Also used to dictate when the hover outline appears.
    /// </summary>
    public static float WeaponPickupRange => defaultPickupRange * (luckySprings + 1);
    /// <summary>
    /// The amount of seconds of bullet-time you get per kill.
    /// </summary>
    public static float BulletTimePerKill => 0.5F * luckySandbags;
    /// <summary>
    /// The bonus multiplier to apply to the player's shooting speed.
    /// </summary>
    public static float ShootSpeedBonus => 1.0F + (0.2F * luckyRings);

    /// <summary>
    /// Call this when the player collects a lucky object of the given type.
    /// </summary>
    /// <param name="luckyObject">The lucky object.</param>
    /// <param name="amount">The amount of them; default is 1.</param>
    public static void Collect(LuckyObject luckyObject, int amount = 1)
    {
        switch (luckyObject)
        {
            case LuckyObject.Clover:
                luckyClovers += amount;
                return;
            case LuckyObject.Rock:
                luckyRocks += amount;
                return;
            case LuckyObject.Spring:
                luckySprings += amount;
                return;
            case LuckyObject.Sandbag:
                luckySandbags += amount;
                return;
            case LuckyObject.Ring:
                luckyRings += amount;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(luckyObject), luckyObject, null);
        }
    }

    /// <summary>
    /// Get the count of a specific LuckyObject collected by the player.
    /// </summary>
    /// <param name="luckyObject">The LuckyObject to get the count for.</param>
    /// <returns>The count of the specified LuckyObject.</returns>
    public static int GetCount(LuckyObject luckyObject)
    {
        return luckyObject switch
        {
            LuckyObject.Clover => luckyClovers,
            LuckyObject.Rock => luckyRocks,
            LuckyObject.Spring => luckySprings,
            LuckyObject.Sandbag => luckySandbags,
            LuckyObject.Ring => luckyRings,
            _ => throw new ArgumentOutOfRangeException(nameof(luckyObject), luckyObject, null)
        };
    }
    
    public static string GetName(this LuckyObject luckyObject)
    {
        return luckyObject switch
        {
            LuckyObject.Clover => "Lucky Clover",
            LuckyObject.Rock => "Lucky Rock",
            LuckyObject.Spring => "Lucky Spring",
            LuckyObject.Sandbag => "Lucky Sandbag",
            LuckyObject.Ring => "Lucky Ring",
            _ => throw new ArgumentOutOfRangeException(nameof(luckyObject), luckyObject, null)
        };
    }
    public static string GetDescription(this LuckyObject luckyObject)
    {
        return luckyObject switch
        {
            LuckyObject.Clover => "Rolls weapon rarity an extra time for each clover. Chooses the best result.",
            LuckyObject.Rock => "Increases the damage of thrown weapons [E] by 50% for each rock.",
            LuckyObject.Spring => "Increases item pickup range for every spring. You will dash towards items that are further away.",
            LuckyObject.Sandbag => "Grants half a second of bullet time on kill for each sandbag.",
            LuckyObject.Ring => "Increases shooting speed by 20% for each ring.",
            _ => throw new ArgumentOutOfRangeException(nameof(luckyObject), luckyObject, null)
        };
    }
    public static void Reset()
    {
        luckyClovers = 0;
        luckyRocks = 0;
        luckySprings = 0;
        luckySandbags = 0;
        luckyRings = 0;
    }
}
public enum LuckyObject
{
    Clover,
    Rock,
    Spring,
    Sandbag,
    Ring
}