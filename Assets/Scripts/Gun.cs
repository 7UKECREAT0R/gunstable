
using System;
using UnityEngine;

/// <summary>
/// Represents a weapon that can be held.
/// </summary>
public readonly struct Gun
{
    public Sprite LoadSprite()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Gun/" + this.sprite);
        if (sprites.Length < 1)
            throw new Exception($"Gun sprite \"{this.sprite}\" does not exist.");
        return sprites[0];
    }
    
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
    /// The sprite of the weapon.
    /// </summary>
    private readonly string sprite;
    /// <summary>
    /// The offset (+ is forward) the barrel is located on this gun sprite.
    /// </summary>
    public readonly float shootPointOffset;
    /// <summary>
    /// The offset (+ is forward) the gun should be located relative to the player.
    /// </summary>
    public readonly float locationOffset;
    /// <summary>
    /// The distance to kick the player and gun back on shoot, in pixels.
    /// </summary>
    public readonly float kickback;
    
    /// <summary>
    /// Cooldown between each shot, in seconds.
    /// </summary>
    private readonly float cooldown;
    /// <summary>
    /// The damage of each projectile, or the hitscan shot if enabled.
    /// </summary>
    private readonly int damage;

    public float Cooldown => this.cooldown * this.rarity.GetCooldownMultiplier();
    public int Damage => Mathf.RoundToInt(this.damage * this.rarity.GetDamageMultiplier());
    public float InterestTime => Game.baseInterestTime * this.rarity.GetInterestMultiplier();
    
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
    /// <param name="shootPointOffset">The offset (+ is forward) the barrel is located on this gun sprite.</param>
    /// <param name="locationOffset">The offset (+ is forward) the gun should be located relative to the player.</param>
    /// <param name="kickback">The distance to kick the player and gun back on shoot.</param>
    public Gun(RarityType rarity, string name, bool isAuto, bool isHitscan,
        float cooldown, float inaccuracy, int projectileCount, float projectileSpeed, int damage,
        string sprite, float shootPointOffset, float locationOffset, float kickback)
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
        this.shootPointOffset = shootPointOffset;
        this.locationOffset = locationOffset;
        this.kickback = kickback;
    }
    public bool Equals(Gun other)
    {
        return this.rarity == other.rarity && this.name == other.name && this.isAuto == other.isAuto &&
               this.isHitscan == other.isHitscan && this.cooldown.Equals(other.cooldown) &&
               this.inaccuracy.Equals(other.inaccuracy) && this.projectileCount == other.projectileCount &&
               this.projectileSpeed.Equals(other.projectileSpeed) && this.damage == other.damage &&
               this.sprite == other.sprite && this.shootPointOffset.Equals(other.shootPointOffset) &&
               this.locationOffset.Equals(other.locationOffset) && this.kickback.Equals(other.kickback);
    }

    public override bool Equals(object obj)
    {
        return obj is Gun other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add((int)this.rarity);
        hashCode.Add(this.name);
        hashCode.Add(this.isAuto);
        hashCode.Add(this.isHitscan);
        hashCode.Add(this.cooldown);
        hashCode.Add(this.inaccuracy);
        hashCode.Add(this.projectileCount);
        hashCode.Add(this.projectileSpeed);
        hashCode.Add(this.damage);
        hashCode.Add(this.sprite);
        hashCode.Add(this.shootPointOffset);
        hashCode.Add(this.locationOffset);
        hashCode.Add(this.kickback);
        return hashCode.ToHashCode();
    }
}