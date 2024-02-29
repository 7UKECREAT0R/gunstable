using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Represents a generic character in the game.
/// </summary>
public abstract class Character : MonoBehaviour
{
    protected enum LookDirection
    {
        Up,
        Down,
        Left,
        Right
    }
    [Pure]
    protected static LookDirection FromDirection(Vector2 direction)
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absX > absY)
        {
            // horizontal vector
            if (direction.x > 0F)
                return LookDirection.Right;
            return LookDirection.Left;
        }

        // vertical vector
        if (direction.y > 0F)
            return LookDirection.Up;
        return LookDirection.Down;
    }
    
    public bool HasGun => this.gun.HasValue;
    
    public abstract bool DropsWeapon { get; }

    protected Gun? gun = null;
    protected LookDirection direction;
    protected SpriteRenderer spriteRenderer;
    public int health;
    public int maxHealth;

    public void SetHP(int hp)
    {
        this.maxHealth = hp;
        this.health = hp;
    }
    
    /// <summary>
    /// Called when this character should die.
    /// </summary>
    public abstract void Die();
    /// <summary>
    /// Called after damage is dealt to the character. This event is called before <see cref="Die"/>
    /// </summary>
    /// <param name="damageAmount">The amount of damage dealt.</param>
    /// <param name="died">If the damage killed this character and <see cref="Die"/> is about to be called.</param>
    public abstract void AfterDamage(int damageAmount, bool died);

    public virtual void Start()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
