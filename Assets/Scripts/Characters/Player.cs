﻿using System;
using System.Collections;
using Shooting;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Characters
{
    public class Player : Character
    {
        private const int HP = 5;
        
        public float movementSpeed = 1.0F;
        private float clickWolfTimer = 0.0F;
        private float interestMax = 0.0F;
        private float interestRemaining = 0.0F;
        
        private Animator animator;
        private bool isMoving;

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

        protected override void OnDeath(Vector2 incomingDirection)
        {
            Game.Reset();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
            this.interestMax = 0F;
        }
        protected override void OnGunEquipped(Gun gun)
        {
            this.interestMax = gun.InterestTime;
            this.interestRemaining = this.interestMax;
        }
        protected override void OnHealthChanged(int oldValue, int newValue)
        {
            this.ui.HealthDisplay = newValue;
        }

        public override void Start()
        {
            base.Start();
            this.cam = Camera.main;
            this.animator = GetComponent<Animator>();
            this.maxHealth = HP;
            this.health = HP;
            
            this.ui = FindObjectOfType<UIDriver>();
            this.ui.HealthDisplay = this.health;
        }
        public override void Update()
        {
            base.Update();
            float deltaTime = Time.deltaTime;
            
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

            if (this.isMoving)
            {
                movementVector.Normalize();
                movementVector *= this.movementSpeed;
                movementVector *= deltaTime;
                this.transform.Translate(movementVector);
            }

            if (this.isMoving != this.lastIsMoving)
                UpdateAnimator();

            this.lastIsMoving = this.isMoving;

            // look direction
            Vector2 mousePosition = this.cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 facing = mousePosition - (Vector2) this.transform.position;
            float angle = Vector2.SignedAngle(Vector2.right, facing);
            this.GunPointAngle = angle;
            
            // interest ticking
            if (this.HasGun)
            {
                if (this.interestRemaining > 0F)
                {
                    this.interestRemaining -= deltaTime;
                    this.ui.InterestPercent = this.interestRemaining / this.interestMax;
                }

                if (this.interestRemaining < 0F)
                {
                    TryThrowGun(angle);
                    this.interestRemaining = 0F;
                }
            }
            else
                this.ui.InterestPercent = 0F;

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
            {
                SpawnGunPickup(mousePosition, Game.RollGun());
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                GlobalStuff stuff = GlobalStuff.SINGLETON;
                stuff.SpawnEnemy(mousePosition, 20, Enemy.SpriteSheet.chef);
            }
            
            // throwing gun
            if (Input.GetKeyDown(KeyCode.E))
                TryThrowGun(angle);
        }
        private void TryThrowGun(float angle)
        {
            if (!this.Gun.HasValue)
                return;
            
            Gun gun = this.Gun.Value;
            this.Gun = null;

            GameObject thrownObject = Instantiate(this.thrownGunPrefab);
            thrownObject.transform.position = GlobalPositionAlongGunDirection(gun.locationOffset);
            thrownObject.transform.rotation = Quaternion.Euler(0F, 0F, angle);
            ThrownGun thrownGun = thrownObject.GetComponent<ThrownGun>();

            //float interest = 1.0F - this.interestRemaining / this.interestMax;
            float projectileMultiplier = gun.isHitscan ? 1.0F : gun.projectileCount;
            float damageFloat = gun.Damage * projectileMultiplier * Game.ThrownWeaponMultiplier /* * interest*/;
            thrownGun.ignoreTag = this.gameObject.tag;
            thrownGun.damage = Mathf.RoundToInt(damageFloat);
            thrownGun.angleOfTravel = angle;
            thrownGun.speed = ThrownGun.SPEED;

            Shake shake = new Shake(1.5F, 20F, 0.3F);
            
            GlobalStuff effects = GlobalStuff.SINGLETON;
            effects.StartShake(shake);
            effects.ImpulseCamera(LocalPositionAlongGunDirection(5F));
        }
        
        private bool isInHitFlash = false;
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