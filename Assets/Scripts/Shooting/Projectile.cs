using System;
using UnityEngine;

namespace Shooting
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        /// <summary>
        /// The lifetime of this projectile.
        /// </summary>
        public float lifetime = 5F;
        
        /// <summary>
        /// The angle this projectile is travelling.
        /// </summary>
        public float angleOfTravel;
        /// <summary>
        /// The speed of this projectile.
        /// </summary>
        public float speed;
        /// <summary>
        /// The damage this projectile will deal on impact.
        /// </summary>
        public int damage;
        /// <summary>
        /// The number of pierces this projectile has left
        /// </summary>
        public int pierce;
        
        public int ignoreLayer;

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            this.lifetime -= deltaTime;
            if (this.lifetime < 0.0F)
            {
                Destroy(this.gameObject);
                return;
            }
            
            this.transform.localRotation = Quaternion.Euler(0F, 0F, this.angleOfTravel);

            float angleRadians = this.angleOfTravel / (180F / Mathf.PI);
            Vector2 movement = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            movement *= this.speed;
            movement *= deltaTime;

            var t = this.transform;
            Vector2 position = t.position;
            position += movement;
            t.position = position;
        }
        private void OnCollisionEnter(Collision other)
        {
            // hit something with a rigid-body
            GameObject hit = other.gameObject;

            // respect the ignoreLayer
            if (hit.layer == this.ignoreLayer)
                return;
            
            Character character = hit.GetComponent<Character>();
            if (character == null)
                return;
            
            // damage the character
            character.Damage(this.damage);

            if (this.pierce > 0)
            {
                this.pierce--;
                return;
            }
            
            Destroy(this.gameObject);
        }
    }
}