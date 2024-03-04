using Characters;
using UnityEngine;

namespace Shooting
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ThrownGun : Projectile
    {
        public Gun Gun
        {
            set
            {
                if (this.gun.GetHashCode() == value.GetHashCode())
                    return; // guns are the same
                
                // set sprite
                this.gun = value;
                this.spriteRenderer.sprite = value.LoadSprite();
            }
        }

        private const float DRAG = 5.0F;
        private float spinDirection;
        private bool waitingToBeDestroyed;
        private Gun gun;
        private SpriteRenderer spriteRenderer;
        
        private void Start()
        {
            this.spriteRenderer = GetComponent<SpriteRenderer>();
            this.spinDirection = (Random.Range(0, 2) == 1 ? 1F : -1F) + Random.Range(-0.1F, 0.1F);
        }
        private void Update()
        {
            float deltaTime = Time.deltaTime;

            this.speed = Mathf.Lerp(this.speed, 0F, DRAG * deltaTime);
            float spin = this.speed * deltaTime * 360F * this.spinDirection;
            this.transform.Rotate(0F, 0F, spin);

            if (this.waitingToBeDestroyed || this.speed > 0.0001F)
                return;
            
            Destroy(this.gameObject, 2.5F);
            this.waitingToBeDestroyed = true;
        }
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            // hit something with a rigid-body
            GameObject hit = other.gameObject;

            Debug.Log($"collision here. ignoring {this.ignoreTag}. hit tag: {hit.tag} name \"{hit.name}\"");
            
            // respect the ignoreTag
            if (!string.IsNullOrEmpty(this.ignoreTag) && hit.CompareTag(this.ignoreTag))
                return;
            
            Debug.Log("collision here");

            // bounce off whatever it hit
            this.angleOfTravel += 180F;
            this.speed /= 1.5F;

            Character character = hit.GetComponent<Character>();
            if (character == null)
                return;
            
            // damage the character
            character.Damage(this.damage);
            Destroy(this.gameObject);
        }
    }
}