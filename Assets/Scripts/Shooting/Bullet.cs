using Characters;
using UnityEditor.Rendering;
using UnityEngine;

namespace Shooting
{
    public class Bullet : Projectile
    {
        /// <summary>
        /// The lifetime of this bullet.
        /// </summary>
        public float lifetime = 5F;
        /// <summary>
        /// The number of pierces this projectile has left
        /// </summary>
        public int pierce;

        private void Update()
        {
            this.lifetime -= Time.deltaTime;

            if (this.lifetime > 0.0F)
                return;
            
            Destroy(this.gameObject);
        }
        protected override void FixedUpdate()
        {
            this.transform.localRotation = Quaternion.Euler(0F, 0F, this.angleOfTravel);
            base.FixedUpdate();
        }
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            // hit something with a rigid-body
            GameObject hit = other.gameObject;

            // respect the ignoreTag
            if (!string.IsNullOrEmpty(this.ignoreTag) && hit.CompareTag(this.ignoreTag))
                return;
            
            Character character = hit.GetComponent<Character>();
            if (character == null)
                return;
            
            // damage the character
            Vector2 attackDirection = (character.transform.position - this.transform.position).normalized;
            character.Damage(this.damage, attackDirection);

            if (this.pierce > 0)
            {
                this.pierce--;
                return;
            }
            
            Destroy(this.gameObject);
        }
    }
}