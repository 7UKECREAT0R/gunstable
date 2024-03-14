/// <summary>
/// Collection of all the guns defined in the game.
/// </summary>
public static class Guns
{
    private static readonly Gun BasicPistol = new(
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
    private static readonly Gun NerfBlaster = new(
        name: "Nerf Blaster",
        damage: 2,
        cooldown: 0.1F,
        inaccuracy: 10F,
        kickback: 3F,
        projectileCount: 1,
        projectileSpeed: 4.0F,
        sprite: "pistol002",
        shootPointOffset: 0.1F,
        locationOffset: 0.07F,
        isAuto: false,
        isHitscan: false);
    private static readonly Gun InjectorPistol = new(
        name: "Injector Pistol",
        damage: 5,
        cooldown: 0.6F,
        inaccuracy: 2F,
        kickback: 5F,
        projectileCount: 1,
        projectileSpeed: 6.5F,
        sprite: "pistol003",
        shootPointOffset: 0.1F,
        locationOffset: 0.08F,
        isAuto: false,
        isHitscan: false);
    private static readonly Gun BigHunka = new(
        name: "Big Hunka",
        damage: 8,
        cooldown: 0.8F,
        inaccuracy: 15F,
        kickback: 10F,
        projectileCount: 1,
        projectileSpeed: 4.0F,
        sprite: "pistol004",
        shootPointOffset: 0.1F,
        locationOffset: 0.08F,
        isAuto: false,
        isHitscan: false);
    
    private static readonly Gun Scopey = new(
        name: "Scopey",
        damage: 12,
        cooldown: 1.5F,
        inaccuracy: 0F,
        kickback: 20F,
        projectileCount: 1,
        projectileSpeed: 8F,
        sprite: "sniper001",
        shootPointOffset: 0.15F,
        locationOffset: 0.09F,
        isAuto: false,
        isHitscan: false);
    private static readonly Gun NailSniper = new(
        name: "Nail Sniper",
        damage: 7,
        cooldown: 0.65F,
        inaccuracy: 0F,
        kickback: 10F,
        projectileCount: 1,
        projectileSpeed: 8F,
        sprite: "sniper002",
        shootPointOffset: 0.15F,
        locationOffset: 0.09F,
        isAuto: false,
        isHitscan: false);

    private static readonly Gun SuperShotgun = new(
        name: "Super Shotgun",
        damage: 2,
        cooldown: 0.5F,
        inaccuracy: 20F,
        kickback: 8F,
        projectileCount: 4,
        projectileSpeed: 4.5F,
        sprite: "shotgun001",
        shootPointOffset: 0.1F,
        locationOffset: 0.08F,
        isAuto: false,
        isHitscan: false);
    private static readonly Gun SawedOff = new(
        name: "Sawed Off",
        damage: 2,
        cooldown: 0.75F,
        inaccuracy: 20F,
        kickback: 8F,
        projectileCount: 6,
        projectileSpeed: 4.5F,
        sprite: "shotgun002",
        shootPointOffset: 0.08F,
        locationOffset: 0.05F,
        isAuto: false,
        isHitscan: false);

    internal static readonly Gun AssaultRifle = new(
        name: "Assault Rifle",
        damage: 2,
        cooldown: 0.235F,
        inaccuracy: 5F,
        kickback: 4F,
        projectileCount: 1,
        projectileSpeed: 5.0F,
        sprite: "rifle001",
        shootPointOffset: 0.1F,
        locationOffset: 0.08F,
        isAuto: true,
        isHitscan: false);
    private static readonly Gun Uzi = new(
        name: "Uzi",
        damage: 1,
        cooldown: 0.1F,
        inaccuracy: 10F,
        kickback: 2.5F,
        projectileCount: 1,
        projectileSpeed: 4.5F,
        sprite: "rifle002",
        shootPointOffset: 0.08F,
        locationOffset: 0.05F,
        isAuto: true,
        isHitscan: false);
    private static readonly Gun TommyGun = new(
        name: "Tommy Gun",
        damage: 3,
        cooldown: 0.2F,
        inaccuracy: 5F,
        kickback: 6F,
        projectileCount: 1,
        projectileSpeed: 5.0F,
        sprite: "rifle003",
        shootPointOffset: 0.1F,
        locationOffset: 0.08F,
        isAuto: true,
        isHitscan: false);
    
    public static readonly Gun[] ALL_GUNS =
    {
        BasicPistol,
        NerfBlaster,
        InjectorPistol,
        BigHunka,
        Scopey,
        NailSniper,
        SuperShotgun,
        SawedOff,
        AssaultRifle,
        Uzi,
        TommyGun
    };
}