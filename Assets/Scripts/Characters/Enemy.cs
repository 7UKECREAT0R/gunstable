using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Characters
{
    public class Enemy : Character
    {
        public enum SpriteSheet
        {
            chef = 0,
            thug = 1,
            politician = 2
        }
        public enum SpriteVariant
        {
            Variant1 = 0,
            Variant2 = 4,
            Variant3 = 8
        }
        
        private readonly RaycastHit2D[] playerVision = new RaycastHit2D[20];
        private const float BULLET_SPEED_MULTIPLIER = 0.5F;
        private const float VISION_CONE = 100F;
        private const float VISION_DISTANCE = 0.5F;
        private const float LOOK_SPEED = 180F;
        private Sprite[] possibleSprites;
        private bool isAlert;
        private Player thePlayer;

        public SpriteSheet spriteSheet;
        public SpriteVariant spriteVariant;

        public void SetSprite(SpriteSheet sheet, SpriteVariant variant, LookDirection direction)
        {
            // reload sprite sheet if needed
            if (this.possibleSprites == null || this.spriteSheet != sheet)
            {
                this.possibleSprites = Resources.LoadAll<Sprite>("Sprites/Enemies/" + sheet);
                goto skipCheck;
            }

            // only update if a change occurred
            if (this.spriteSheet != sheet && this.spriteVariant == variant && this.direction == direction)
                return;
            
            skipCheck:
            this.spriteSheet = sheet;
            this.spriteVariant = variant;
            this.direction = direction;
            this.spriteRenderer.sprite = this.possibleSprites[(int)this.spriteVariant * 4 + (int)this.direction];
        }
        public void SetSpriteForce(SpriteSheet sheet, SpriteVariant variant, LookDirection direction)
        {
            // reload sprite sheet if needed
            if (this.possibleSprites == null || this.spriteSheet != sheet)
                this.possibleSprites = Resources.LoadAll<Sprite>("Sprites/Enemies/" + sheet);
            
            this.spriteSheet = sheet;
            this.spriteVariant = variant;
            this.direction = direction;
            this.spriteRenderer.sprite = this.possibleSprites[(int)this.spriteVariant * 4 + (int)this.direction];
        }
        public override void Start()
        {
            base.Start();
            this.thePlayer = GameObject
                .Find("Player")
                .GetComponent<Player>();
            
            float angleRadians = this.GunPointAngle / (180F / Mathf.PI);
            Vector2 movement = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            this.direction = FromDirection(movement);
            
            SetSpriteForce(this.spriteSheet, this.spriteVariant, this.direction);

            this.Gun = Guns.BasicPistol;
        }
        public void FixedUpdate()
        {
            if (Game.isPaused)
                return;
            
            // shoot a ray towards the player, ignoring objects with the same tag.
            var positionTransform = this.transform;
            var position = positionTransform.position;
            Vector2 directionToPlayer = (this.thePlayer.transform.position - position).normalized;
            float angleToPlayer = Vector2.SignedAngle(Vector2.right, directionToPlayer);

            // cancel ray cast if out of the view, or already alert
            if (!this.isAlert && Mathf.Abs(Mathf.DeltaAngle(angleToPlayer, this.GunPointAngle)) < (VISION_CONE / 2F))
            {
                int size = Physics2D.RaycastNonAlloc(position, directionToPlayer, this.playerVision, VISION_DISTANCE);
                for (int i = 0; i < size; i++)
                {
                    RaycastHit2D hit = this.playerVision[i];
                    GameObject hitObject = hit.transform.gameObject;

                    if (hitObject.CompareTag(this.gameObject.tag))
                        continue; // fellow enemy, keep looking
                    if (!hitObject.TryGetComponent(typeof(Player), out _))
                        break; // wall or other non-player object. stop the cast.

                    this.isAlert = true;
                }
            }

            if (!this.isAlert)
                return;
            
            float currentAngle = this.GunPointAngle;
            float goalAngle = Vector2.SignedAngle(Vector2.right, directionToPlayer);
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, goalAngle, LOOK_SPEED * Time.fixedDeltaTime);
            this.GunPointAngle = newAngle;
            
            LookDirection direction = FromDirection(directionToPlayer);
            SetSprite(this.spriteSheet, this.spriteVariant, direction);
            
            // shoot at player if it can
            if (!this.GunCanShoot || !this.Gun.HasValue)
                return;

            // shoot
            ShootPointAngle(this.GunPointAngle, BULLET_SPEED_MULTIPLIER);
            
            // little delay to next fire
            this.shootCooldown += this.Gun.Value.Cooldown * Random.Range(0.5F, 2.0F);
        }

        public override bool IsPlayer => false;

        protected override void OnDeath(Vector2 incomingDirection)
        {
            bool shouldDropGun = Game.RollDropType();
            if (shouldDropGun)
            {
                Gun gunToDrop = Game.RollGun();
                SpawnGunPickup(this.transform.position, gunToDrop);
            }
            else
            {
                LuckyObject objectToDrop = Game.RollLuckyObject();
                SpawnLuckyObjectPickup(this.transform.position, objectToDrop);
            }
            
            GlobalStuff stuff = GlobalStuff.SINGLETON;
            GameObject bloodParticles = Instantiate(stuff.bloodParticlePrefab);
            bloodParticles.transform.position = this.transform.position;
            bloodParticles.transform.forward = incomingDirection;
            stuff.ActivateBulletTime(1F);
            stuff.RemoveEnemy(this);
            Destroy(this.gameObject);
        }
        protected override void AfterDamage(int damageAmount, bool died)
        {
            this.isAlert = true;
            
            GlobalStuff effects = GlobalStuff.SINGLETON;
            float colorR = 1.0F;
            colorR *= 1.0F - 0.02F * damageAmount;
            
            effects.CreateActionText(
                this.transform.position + (Vector3)(Vector2.up * 0.15F),
                damageAmount.ToString(),
                new Color(colorR, 0F, 0F, 1F),
                new Color(colorR, 0.3F, 0.3F, 1F),
                0.4F,
                10.0F);

            if (died)
                return;
            
            if(this.isInHitFlash)
                StopAllCoroutines();
            StartCoroutine(DoHitFlash());
        }

        private bool isInHitFlash = false;
        private const float HIT_FLASH_SECONDS = 0.1F;
        private IEnumerator DoHitFlash()
        {
            GlobalStuff stuff = GlobalStuff.SINGLETON;
            this.spriteRenderer.material = stuff.hitFlashOther;
            this.isInHitFlash = true;
            
            yield return new WaitForSeconds(HIT_FLASH_SECONDS);
            
            // ReSharper disable once Unity.InefficientPropertyAccess
            this.spriteRenderer.material = stuff.spriteLitDefault;
            this.isInHitFlash = false;
        }
        
        protected override void OnGunUnequipped() {}
        protected override void OnGunEquipped(Gun gun) {}
        protected override void OnHealthChanged(int oldValue, int newValue) {}
    }
}