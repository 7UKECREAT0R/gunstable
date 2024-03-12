using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Rarity
{
    /// <summary>
    /// Roll rarity N number of times, and return the best one.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public static RarityType Roll(int amount)
    {
        RarityType best = RarityType.Unremarkable;
        
        for (int i = 0; i < amount; i++)
        {
            RarityType rolledRarity = RollSingle();
            if (rolledRarity > best)
                best = rolledRarity;
        }
        
        return best;
    }
    /// <summary>
    /// Roll for a single rarity type with the default luck parameters.
    /// </summary>
    /// <returns></returns>
    private static RarityType RollSingle()
    {
        int percent = Random.Range(0, 100);

        return percent switch
        {
            < 80 => RarityType.Unremarkable,
            < 92 => RarityType.Cool,
            < 97 => RarityType.DoubleTake,
            < 99 => RarityType.TripleTake,
            _ => RarityType.Unbelievable
        };
    }

    public static Color GetColor(this RarityType rarity)
    {
        return rarity switch
        {
            RarityType.Unremarkable => RGB(204, 204, 204),
            RarityType.Cool => RGB(59, 227, 143),
            RarityType.DoubleTake => RGB(59, 221, 227),
            RarityType.TripleTake => RGB(182, 59, 227),
            RarityType.Unbelievable => RGB(227, 59, 109),
            _ => Color.magenta
        };
        
        Color RGB(int r, int g, int b) => new(r / 255f, g / 255f, b / 255f);
    }
    public static float GetDamageMultiplier(this RarityType rarity)
    {
        return rarity switch
        {
            RarityType.Unremarkable => 1.0F,
            RarityType.Cool => 1.1F,
            RarityType.DoubleTake => 1.25F,
            RarityType.TripleTake => 1.5F,
            RarityType.Unbelievable => 2.0F,
            _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
        };
    }
    public static float GetCooldownMultiplier(this RarityType rarity)
    {
        return rarity switch
        {
            RarityType.Unremarkable => 1.0F - 0.0F,
            RarityType.Cool => 1.0F - 0.03F,
            RarityType.DoubleTake => 1.0F - 0.06F,
            RarityType.TripleTake => 1.0F - 0.1F,
            RarityType.Unbelievable => 1.0F - 0.25F,
            _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
        };
    }
    public static float GetInterestMultiplier(this RarityType rarity)
    {
        return rarity switch
        {
            RarityType.Unremarkable => 1.0F,
            RarityType.Cool => 1.25F,
            RarityType.DoubleTake => 1.5F,
            RarityType.TripleTake => 2.0F,
            RarityType.Unbelievable => 3.5F,
            _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
        };
    }
}
public enum RarityType
{
    Unremarkable = 0,
    Cool = 1,
    DoubleTake = 2,
    TripleTake = 3,
    Unbelievable = 4
}