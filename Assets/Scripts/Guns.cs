/// <summary>
/// Collection of all the guns defined in the game.
/// </summary>
public static class Guns
{
    public static readonly Gun BasicPistol = new Gun(
        rarity: RarityType.Unremarkable,
        name: "Basic Pistol",
        damage: 3,
        cooldown: 0.25F,
        inaccuracy: 2F,
        kickback: 5F,
        projectileCount: 1,
        projectileSpeed: 5.0F,
        sprite: "pistol001",
        shootPointOffset: 0.1F,
        locationOffset: 0.08F,
        isAuto: false,
        isHitscan: false);

    public static readonly Gun[] ALL_GUNS =
    {
        BasicPistol
    };
}