
/// <summary>
/// Represents a weapon that can be held.
/// </summary>
public readonly struct Gun
{
    /// <summary>
    /// The rarity of the gun.
    /// </summary>
    public readonly RarityType rarity;
    /// <summary>
    /// The name of the gun.
    /// </summary>
    public readonly string name;
    
    /// <summary>
    /// Fire while the mouse is held?
    /// </summary>
    public readonly bool isAuto;
    /// <summary>
    /// Does weapon use ray instead of firing projectiles?
    /// </summary>
    public readonly bool isHitscan;
    /// <summary>
    /// Cooldown between each shot, in seconds.
    /// </summary>
    public readonly float cooldown;
    /// <summary>
    /// The degrees of inaccuracy of each projectile, if not hitscan.
    /// </summary>
    public readonly float inaccuracy;
    /// <summary>
    /// Number of projectiles to fire. if not hitscan.
    /// </summary>
    public readonly int projectileCount;
    /// <summary>
    /// The speed of the fired projectiles, if not hitscan.
    /// </summary>
    public readonly float projectileSpeed;
    /// <summary>
    /// The damage of each projectile, or the hitscan shot if enabled.
    /// </summary>
    public readonly int damage;

    /// <summary>
    /// The sprite of the weapon.
    /// </summary>
    public readonly string sprite;

    /// <summary>
    /// Create a new Gun definition.
    /// </summary>
    /// <param name="rarity">The rarity of the gun.</param>
    /// <param name="name">The name of the gun.</param>
    /// <param name="isAuto">Fire while the mouse is held?</param>
    /// <param name="isHitscan">Does weapon use ray instead of firing projectiles?</param>
    /// <param name="cooldown">Cooldown between each shot, in seconds.</param>
    /// <param name="inaccuracy">The degrees of inaccuracy of each projectile, if not hitscan.</param>
    /// <param name="projectileCount">Number of projectiles to fire, if not hitscan.</param>
    /// <param name="projectileSpeed">The speed of the fired projectiles, if not hitscan.</param>
    /// <param name="damage">The damage of each projectile, or the hitscan shot if enabled.</param>
    /// <param name="sprite">The sprite of the weapon.</param>
    public Gun(RarityType rarity, string name, bool isAuto, bool isHitscan,
        float cooldown, float inaccuracy, int projectileCount, float projectileSpeed,
        int damage, string sprite)
    {
        this.rarity = rarity;
        this.name = name;
        this.isAuto = isAuto;
        this.isHitscan = isHitscan;
        this.cooldown = cooldown;
        this.inaccuracy = inaccuracy;
        this.projectileCount = projectileCount;
        this.projectileSpeed = projectileSpeed;
        this.damage = damage;
        this.sprite = sprite;
    }
}