using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using JetBrains.Annotations;
using Shooting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    /// <summary>
    /// Represents a generic character in the game.
    /// </summary>
    public abstract class Character : MonoBehaviour
    {
        public enum LookDirection
        {
            Right,
            Up,
            Left,
            Down
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

        public abstract bool IsPlayer { get; }
        public bool HasGun => this.gun.HasValue;

        /// <summary>
        /// Gets/Sets the current gun. Setting this field will send out <see cref="OnGunEquipped"/> and set the sprite of the display gun.
        /// Setting this field to null will unequip the gun and send out <see cref="OnGunUnequipped"/>
        /// </summary>
        public Gun? Gun
        {
            get => this.gun;
            set
            {
                if (!value.HasValue && !this.gun.HasValue)
                    return; // don't do anything, no gun already.

                this.gunRenderer.enabled = value.HasValue;
                this.gun = value;

                if (!value.HasValue)
                {
                    OnGunUnequipped();
                    return;
                }
                
                this.gunDistance = value.Value.locationOffset;
                this.gunRenderer.sprite = value.Value.LoadSprite();
                OnGunEquipped(value.Value);
            }
        }

        protected const float gunOffsetLerp = 16.0F;
        protected const float velocityLerp = 10.0F;

        public GameObject bulletProjectilePrefab;
        public GameObject thrownGunPrefab;
        public GameObject gunItemPrefab;
        public GameObject luckyItemPrefab;

        private Gun? gun = null;
        protected float velocityX, velocityY;
        protected LookDirection direction;
        protected SpriteRenderer spriteRenderer;

        public SpriteRenderer gunRenderer;
        public int health;
        public int maxHealth;

        public bool IsDead => this.health < 1;

        protected float GunDistanceOffset
        {
            get => this.gunDistanceOffset;
            set
            {
                this.gunDistanceOffset = value;
                RepositionGun();
            }
        }
        public float GunPointAngle
        {
            get => this.gunPointAngle;
            set
            {
                value %= 360F;
                
                bool behind = value > 0F;
                bool flip = value is < -90F or > 90F;
                
                this.gunPointAngle = value;
                this.gunRenderer.transform.localScale = new Vector3(flip ? -1F : 1F, 1F, 1F);

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (value < -90F)
                    value = 180F + value;
                else if (value > 90F)
                    value = 180F + value;

                this.gunRenderer.transform.localRotation = Quaternion.Euler(0F, 0F, value);
                this.gunRenderer.sortingOrder = behind ? 9 : 11;
            }
        }
        protected bool GunCanShoot => this.shootCooldown < 0.0F;
        protected float shootCooldown;

        protected float gunDistanceOffset;
        private float gunDistance;
        private float gunPointAngle;

        /// <summary>
        /// Repositions the gun based on:
        /// <see cref="gunDistanceOffset"/>,
        /// <see cref="gunDistance"/>, and
        /// <see cref="gunPointAngle"/>
        /// </summary>
        protected void RepositionGun()
        {
            float distanceTotal = this.gunDistance + this.gunDistanceOffset;

            if (Mathf.Abs(distanceTotal) < 0.0001F)
            {
                this.gunRenderer.transform.localPosition = Vector2.zero;
                return;
            }

            Vector2 location = LocalPositionAlongGunDirection(distanceTotal);
            this.gunRenderer.transform.localPosition = location;
        }

        /// <summary>
        /// Returns the position along the angle the gun is pointing, in local space.
        /// </summary>
        /// <param name="offset">Positive is forwards, negative is backwards.</param>
        /// <returns>The calculated position.</returns>
        protected Vector2 LocalPositionAlongGunDirection(float offset)
        {
            float radians = this.gunPointAngle / (180F / Mathf.PI);
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * offset;
        }
        /// <summary>
        /// Returns the position along the angle the gun is pointing, in global space.
        /// </summary>
        /// <param name="offset">Positive is forwards, negative is backwards.</param>
        /// <returns>The calculated position.</returns>
        protected Vector2 GlobalPositionAlongGunDirection(float offset)
        {
            float radians = this.gunPointAngle / (180F / Mathf.PI);
            Vector2 vector = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * offset;
            return vector + (Vector2) this.transform.position;
        }

        /// <summary>
        /// Deal damage to this character, possibly causing death or serious injury.
        /// </summary>
        /// <param name="amount">The amount of damage the attack deals.</param>
        /// <param name="theDirection">The direction the attack came from.</param>
        public void Damage(int amount, Vector2 theDirection)
        {
            if (amount < 1)
                return;
            if (this.IsPlayer)
            {
                if (((Player)this).isInHitFlash)
                    return;
                amount = 1; // only take 1 damage at a time
            }

            int oldHealth = this.health;
            this.health -= amount;
            bool dead = this.health < 1;

            OnHealthChanged(oldHealth, this.health);
            AfterDamage(amount, dead);

            GlobalStuff stuff = GlobalStuff.SINGLETON;

            if (dead)
            {
                stuff.PlaySound(Random.Range(0, 1000) == 0 ?
                    GlobalStuff.SoundEffect.KillAlt :
                    GlobalStuff.SoundEffect.Kill);

                OnDeath(theDirection);
            }
            else
                stuff.PlaySound(GlobalStuff.SoundEffect.Hurt);
        }

        public void ImpulseVelocity(Vector2 xy) => ImpulseVelocity(xy.x, xy.y);
        public void ImpulseVelocity(float x, float y)
        {
            this.velocityX += x;
            this.velocityY += y;
        }

        public void SetVelocity(Vector2 xy) => SetVelocity(xy.x, xy.y);
        public void SetVelocity(float x, float y)
        {
            this.velocityX = x;
            this.velocityY = y;
        }

        private void SpawnBullet(Gun gun, Vector2 position, float angle, float slowdownFactor, Collider ignoreCollider = null)
        {
            GameObject spawnedBullet = Instantiate(this.bulletProjectilePrefab);
            spawnedBullet.transform.position = position;
            spawnedBullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            if(ignoreCollider != null)
                Physics.IgnoreCollision(spawnedBullet.GetComponent<Collider>(), ignoreCollider);
            
            Bullet projectile = spawnedBullet.GetComponent<Bullet>();
            projectile.angleOfTravel = angle;
            projectile.speed = gun.projectileSpeed * slowdownFactor;
            projectile.damage = gun.Damage;
            projectile.pierce = 0;
            projectile.ignoreTag = this.gameObject.tag;
        }

        [PublicAPI]
        public void SpawnLuckyObjectPickup(Vector2 position, LuckyObject luckyObject)
        {
            GameObject pickup = Instantiate(this.luckyItemPrefab);
            pickup.transform.position = position;
            LuckyItem item = pickup.GetComponent<LuckyItem>();
            item.luckyObject = luckyObject;
            GlobalStuff.SINGLETON.droppedItems[pickup.GetInstanceID()] = item;
        }
        [PublicAPI]
        public void SpawnGunPickup(Vector2 position, Gun gun)
        {
            GameObject pickup = Instantiate(this.gunItemPrefab);
            pickup.transform.position = position;
            GunItem item = pickup.GetComponent<GunItem>();
            item.gun = gun;
            GlobalStuff.SINGLETON.droppedItems[pickup.GetInstanceID()] = item;
        }
        
        /// <summary>
        /// Fires the currently equipped gun, if any.
        /// </summary>
        protected void ShootPointAngle(float angle, float slowdownFactor = 1.0F, Collider ignoreCollider = null)
        {
            if (!this.gun.HasValue)
                return;

            Gun gun = this.gun.Value;
            float shellOffset = gun.locationOffset;
            float shootPointOffset = gun.locationOffset + gun.shootPointOffset;

            // spawn shell
            Vector2 shellPoint = GlobalPositionAlongGunDirection(shellOffset);
            GlobalStuff stuff = GlobalStuff.SINGLETON;
            stuff.CreateShellParticle(shellPoint);

            // get the shoot point
            Vector2 shootPoint = GlobalPositionAlongGunDirection(shootPointOffset);

            if (gun.isHitscan)
            {
                // no
            }
            else
            {
                bool shotgun = gun.projectileCount > 1;
                
                for (int i = 0; i < gun.projectileCount; i++)
                {
                    float deviation = Random.Range(-gun.inaccuracy / 2F, gun.inaccuracy / 2F);
                    float deviatedAngle = angle + deviation;
                    
                    if(shotgun)
                        stuff.PlaySoundDelayed(GlobalStuff.SoundEffect.Shoot, Random.Range(0F, 0.1F));
                    else
                        stuff.PlaySound(GlobalStuff.SoundEffect.Shoot);

                    float slow = slowdownFactor;
                    if (shotgun)
                        slow -= Random.Range(0F, 0.1F);
                    
                    SpawnBullet(gun, shootPoint, deviatedAngle, slow, ignoreCollider);
                }
            }

            // do knockback
            float kickback = -(gun.kickback / 100F);
            Vector2 knockbackVector = LocalPositionAlongGunDirection(kickback);
            ImpulseVelocity(knockbackVector);
            this.GunDistanceOffset = kickback;
            
            if(this.IsPlayer)
                this.shootCooldown = gun.Cooldown * (1 / Game.ShootSpeedBonus);
            else
                this.shootCooldown = gun.Cooldown;
        }

        /// <summary>
        /// Fires the currently equipped gun, if any, at the given direction.
        /// </summary>
        /// <param name="direction">The direction to fire in.</param>
        /// <param name="slowdownFactor">The amount to slow the bullet down.</param>
        protected void Shoot(Vector2 direction, float slowdownFactor = 1.0F)
        {
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            ShootPointAngle(angle, slowdownFactor);
        }

        /// <summary>
        /// Called when this character should die.
        /// </summary>
        /// <param name="incomingDirection"></param>
        protected abstract void OnDeath(Vector2 incomingDirection);

        /// <summary>
        /// Called after damage is dealt to the character. This event is called just before <see cref="OnDeath"/>
        /// </summary>
        /// <param name="damageAmount">The amount of damage dealt.</param>
        /// <param name="died">If the damage killed this character and <see cref="OnDeath"/> is about to be called.</param>
        protected abstract void AfterDamage(int damageAmount, bool died);

        /// <summary>
        /// Called when the gun is unequipped.
        /// </summary>
        protected abstract void OnGunUnequipped();

        /// <summary>
        /// Called when a gun is equipped.
        /// </summary>
        /// <param name="gun">The newly equipped gun.</param>
        protected abstract void OnGunEquipped(Gun gun);

        /// <summary>
        /// Called when this character's health changes, either up or down.
        /// </summary>
        /// <param name="oldValue">The value before the change occurred.</param>
        /// <param name="newValue">The new value.</param>
        protected abstract void OnHealthChanged(int oldValue, int newValue);

        public virtual void Update()
        {
            float deltaTime = Time.deltaTime;
            this.shootCooldown -= this.IsPlayer ? Time.unscaledDeltaTime : deltaTime;
            this.gunDistanceOffset = Mathf.Lerp(this.gunDistanceOffset, 0.0F, gunOffsetLerp * deltaTime);
            this.velocityX = Mathf.Lerp(this.velocityX, 0.0F, velocityLerp * deltaTime);
            this.velocityY = Mathf.Lerp(this.velocityY, 0.0F, velocityLerp * deltaTime);

            RepositionGun();
            this.transform.Translate(new Vector3(this.velocityX * deltaTime, this.velocityY * deltaTime));
        }

        public virtual void Start()
        {
            this.spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}