using UnityEngine;

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
            < 75 => RarityType.Unremarkable,
            < 90 => RarityType.Cool,
            < 96 => RarityType.DoubleTake,
            < 99 => RarityType.TripleTake,
            _ => RarityType.Unbelievable
        };
    }

    public static Color GetColorForRarity(RarityType rarity)
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
}
public enum RarityType
{
    Unremarkable = 0,
    Cool = 1,
    DoubleTake = 2,
    TripleTake = 3,
    Unbelievable = 4
}