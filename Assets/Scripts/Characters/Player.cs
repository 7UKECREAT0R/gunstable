using System;
using System.Collections;
using Shooting;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldgen;

namespace Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : Character
    {
        private const int HP = 5;
        
        public float movementSpeed = 1.0F;
        private float clickWolfTimer;
        private float interestMax;
        private float interestRemaining;
        
        private Animator animator;
        private bool isMoving;

        private Rigidbody2D cc;
        private LookDirection lastDirection;
        private bool lastIsMoving;
        private Camera cam;
        private UIDriver ui;

        /// <summary>
        /// Look towards the given direction. Returns the un-normalized direction from the player to the given location.
        /// </summary>
        /// <param name="location">The location to look at.</param>
        /// <returns>Direction from the player to the given location, not normalized.</returns>
        public Vector2 LookTowards(Vector2 location)
        {
            Vector2 pos = location - (Vector2) this.transform.position;
            Vector2 diff = pos.normalized;
            this.direction = FromDirection(diff);

            if (this.direction != this.lastDirection)
                UpdateAnimator();

            this.lastDirection = this.direction;
            return pos;
        }

        private void UpdateAnimator()
        {
            string stateName = this.isMoving ? "playerMove" + this.direction : "playerStand" + this.direction;
            this.animator.Play(stateName);
        }

        public override bool IsPlayer => true;
        
        protected override void OnDeath(Vector2 incomingDirection)
        {
            GlobalStuff stuff = GlobalStuff.SINGLETON;
            stuff.ActivateDeath(incomingDirection);
            
        }
        protected override void AfterDamage(int damageAmount, bool died)
        {
            GlobalStuff effects = GlobalStuff.SINGLETON;
            effects.StartShake(new Shake(0.3F, 60.0F, 0.3F));
            
            if (died)
                return;
            
            if(this.isInHitFlash)
                StopAllCoroutines();
            StartCoroutine(DoHitFlash());
        }
        protected override void OnGunUnequipped()
        {
            this.interestRemaining = 0F;
            this.interestMax = 1F;
        }
        protected override void OnGunEquipped(Gun gun)
        {
            this.interestMax = gun.InterestTime;
            this.interestRemaining = this.interestMax;

            if (gun.rarity < RarityType.DoubleTake)
                return;
            if (this.health >= this.maxHealth)
                return;

            int healthGain = (int) gun.rarity - (int) RarityType.Cool;
            
            
            GlobalStuff.SINGLETON.CreateActionText(
                this.transform.position + (Vector3)(Vector2.up * 0.15F),
                "COOL! +" + healthGain,
                new Color(0F, 1F, 0F, 1F),
                new Color(0.3F, 1F, 0.3F, 1F),
                0.4F,
                8.0F);

            this.health += healthGain;
            
            if (this.health > this.maxHealth)
                this.health = this.maxHealth;
            
            this.ui.HealthDisplay = this.health;
        }
        protected override void OnHealthChanged(int oldValue, int newValue)
        {
            this.ui.HealthDisplay = newValue;
        }

        public override void Start()
        {
            base.Start();
            GetComponent<Collider>();
            this.cc = GetComponent<Rigidbody2D>();
            this.cam = Camera.main;
            this.animator = GetComponent<Animator>();
            this.maxHealth = HP;
            this.health = HP;
            
            this.ui = FindObjectOfType<UIDriver>();
        }
        public override void Update()
        {
            float deltaTime = Time.deltaTime;
            this.shootCooldown -= deltaTime;
            this.gunDistanceOffset = Mathf.Lerp(this.gunDistanceOffset, 0.0F, gunOffsetLerp * deltaTime);
            this.velocityX = Mathf.Lerp(this.velocityX, 0.0F, velocityLerp * deltaTime);
            this.velocityY = Mathf.Lerp(this.velocityY, 0.0F, velocityLerp * deltaTime);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Game.isPaused = !Game.isPaused;
                this.ui.PauseMenuIsVisible = Game.isPaused;
            }
            
            if (Game.isPaused)
                return;

            RepositionGun();
            
            Vector2 movementVector = Vector2.zero;
            this.isMoving = false;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                movementVector += Vector2.left;
                this.isMoving = true;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                movementVector += Vector2.right;
                this.isMoving = true;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                movementVector += Vector2.up;
                this.isMoving = true;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                movementVector += Vector2.down;
                this.isMoving = true;
            }
            
            movementVector.Normalize();
            movementVector *= this.movementSpeed;
            movementVector.x += this.velocityX;
            movementVector.y += this.velocityY;
            
            // adjust for time scale so the player so they can move at semi-full speed.
            if (Time.timeScale < 1F)
            {
                // this still lets you "feel" the bullet time, but
                // you still get the benefits of being in it.
                float adjustedScale = Mathf.Lerp(Time.timeScale, 1F, 0.25F);
                movementVector /= adjustedScale;
            }

            // apply movement vector
            this.cc.velocity = movementVector;

            if (this.isMoving != this.lastIsMoving)
                UpdateAnimator();

            this.lastIsMoving = this.isMoving;

            // look direction
            Vector2 mousePosition = this.cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 facing = mousePosition - (Vector2) this.transform.position;
            float angle = Vector2.SignedAngle(Vector2.right, facing);
            this.GunPointAngle = angle;
            
            GlobalStuff stuff = GlobalStuff.SINGLETON;
            
            // interest ticking
            if (this.HasGun)
            {
                if (this.interestRemaining > 0F)
                {
                    this.interestRemaining -= deltaTime;
                    float interestPercent = this.interestRemaining / this.interestMax;
                    this.ui.InterestPercent = interestPercent;
                    this.ui.shaking = interestPercent < 0.1F;
                }

                if (this.interestRemaining < 0F)
                {
                    TryThrowGun(angle);
                    this.interestRemaining = 0F;
                }
            }
            else
            {
                this.ui.shaking = false;
                this.ui.InterestPercent = 0F;
            }

            // shooting
            if (this.Gun.HasValue)
            {
                Gun gun = this.Gun.Value;
                bool shoot;

                if (gun.isAuto)
                    shoot = Input.GetMouseButton(0);
                else
                {
                    if (Input.GetMouseButtonDown(0))
                        this.clickWolfTimer = 0.3F;
                    shoot = this.clickWolfTimer > 0F;
                }
                
                if (shoot && this.GunCanShoot)
                {
                    ShootPointAngle(angle);
                    this.clickWolfTimer = 0F;
                }
            }

            this.clickWolfTimer -= deltaTime;
            
            // random roll keybind
            if (Input.GetKeyDown(KeyCode.F3))
                SpawnGunPickup(mousePosition, Game.RollGun());
            // random enemy keybind
            if (Input.GetKeyDown(KeyCode.F4))
            {
                stuff.SpawnEnemy(mousePosition, 20, angle, Enemy.SpriteSheet.chef);
            }
            // regen world keybind
            if (Input.GetKeyDown(KeyCode.F5))
            {
                World world = FindObjectOfType<World>();
                world.Regenerate();
            }
            // text test keybind
            if (Input.GetKeyDown(KeyCode.F6))
                this.ui.DisplayInfoString("Your current health is " + this.health + ".", 2.0F, Color.white);
            // dmg keybind
            if (Input.GetKeyDown(KeyCode.F8))
                Damage(1, Vector2.down);
            
            // throwing gun
            if (Input.GetKeyDown(KeyCode.E))
                TryThrowGun(angle);
        }
        private void TryThrowGun(float angle)
        {
            if (!this.Gun.HasValue)
                return;
            
            GlobalStuff effects = GlobalStuff.SINGLETON;
            if (effects.NoMoreGuns)
            {
                this.interestRemaining = 0.1F;
                return;
            }

            Gun gun = this.Gun.Value;

            GameObject thrownObject = Instantiate(this.thrownGunPrefab);
            thrownObject.transform.position = GlobalPositionAlongGunDirection(gun.locationOffset);
            thrownObject.transform.rotation = Quaternion.Euler(0F, 0F, angle);

            ThrownGun thrownGun = thrownObject.GetComponent<ThrownGun>();
            const float BASE_DAMAGE_MULTIPLIER = 2.0F;
            float interest = 1F - this.interestRemaining / this.interestMax;

            float projectileMultiplier = gun.isHitscan ? 1.0F : gun.projectileCount;
            float damageFloat = BASE_DAMAGE_MULTIPLIER * gun.Damage * projectileMultiplier * Game.ThrownWeaponMultiplier * interest;
            thrownGun.ignoreTag = this.gameObject.tag;
            thrownGun.damage = Mathf.RoundToInt(damageFloat);
            thrownGun.angleOfTravel = angle;
            thrownGun.speed = ThrownGun.SPEED * interest;
            Shake shake = new Shake(1.5F * interest, 20F, 0.3F);

            effects.StartShake(shake);
            effects.ImpulseCamera(LocalPositionAlongGunDirection(5F * interest));
            this.Gun = null;
            
            if (interest > 0.9F)
                GlobalStuff.SINGLETON.ActivateBulletTime(1.5F);
        }
        
        private bool isInHitFlash;
        private const float HIT_FLASH_SECONDS = 0.1F;
        private IEnumerator DoHitFlash()
        {
            GlobalStuff stuff = GlobalStuff.SINGLETON;
            this.spriteRenderer.material = stuff.hitFlashPlayer;
            this.isInHitFlash = true;
            
            yield return new WaitForSeconds(HIT_FLASH_SECONDS);
            
            // ReSharper disable once Unity.InefficientPropertyAccess
            this.spriteRenderer.material = stuff.spriteLitDefault;
            this.isInHitFlash = false;
        }
    }
}