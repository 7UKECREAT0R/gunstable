using System;
using Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shooting
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Projectile : MonoBehaviour
    {
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
        
        public string ignoreTag;

        protected virtual void Update()
        {
            if (Game.isPaused)
                return;
            
            float angleRadians = this.angleOfTravel / (180F / Mathf.PI);
            Vector2 movement = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            movement *= this.speed;
            movement *= Time.deltaTime;

            var t = this.transform;
            Vector2 position = t.position;
            position += movement;
            t.position = position;
        }
        protected abstract void OnTriggerEnter2D(Collider2D other);
    }
}