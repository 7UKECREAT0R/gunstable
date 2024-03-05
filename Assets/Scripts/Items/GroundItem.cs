using System;
using System.Collections.Generic;
using Characters;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class GroundItem : MonoBehaviour
    {
        private const float SPEED_H = 0.15F;
        private const float SPEED_Y = 0.75F;
        private const float GRAVITY = 4.0F;
        
        private bool hovered;
        private float angleOfTravel;
        private float hVelocity;
        private float yLevelFake;
        private float yVelocity;

        protected SpriteRenderer spriteRenderer;
        protected Camera cam;
        protected Player player;

        private RarityType type = RarityType.Unremarkable;

        protected void SetRarityType(RarityType type)
        {
            this.type = type;
        }
        private void SetHoveredState(bool newHovered)
        {
            if (newHovered != this.hovered)
            {
                if (newHovered)
                    OnHoverStart();
                else
                    OnHoverEnd();
                
                SetMaterial(newHovered);
            }

            this.hovered = newHovered;
        }
        private void SetMaterial(bool hovered)
        {
            GlobalStuff effects = GlobalStuff.SINGLETON;
            
            if (hovered)
            {
                this.spriteRenderer.material = effects.selectionMaterial;
                return;
            }
            this.spriteRenderer.material = effects.GetMaterialForRarity(this.type);
        }
        protected virtual void Start()
        {
            this.player = GameObject.Find("Player").GetComponent<Player>();
            this.cam = Camera.main;
            this.spriteRenderer = GetComponent<SpriteRenderer>();
            this.hovered = false;

            this.angleOfTravel = Random.value * 360F;
            this.hVelocity = SPEED_H;
            this.yVelocity = SPEED_Y;
            this.yLevelFake = 0.05F;
        }

        private bool firstMaterialSet = false;
        private void LateUpdate()
        {
            if (this.firstMaterialSet)
                return;
            this.firstMaterialSet = true;
            SetMaterial(this.hovered);
        }
        protected virtual void Update()
        {
            // moving
            float deltaTime = Time.deltaTime;
            
            float angleRadians = this.angleOfTravel / (180F / Mathf.PI);
            Vector2 movement = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            movement *= this.hVelocity * deltaTime;
            this.yLevelFake += this.yVelocity * deltaTime;
            movement.y += this.yVelocity * deltaTime;

            this.yVelocity -= GRAVITY * deltaTime;
            
            if (this.yLevelFake < 0F)
            {
                this.yLevelFake = 0F;
                this.yVelocity *= -1F;
                this.hVelocity /= 3F;
                this.yVelocity /= 3F;
            }

            Transform change = this.transform;
            Vector2 position = change.position;
            position += movement;
            change.position = position;


            Vector2 mousePosition = this.cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 itemPosition = this.transform.position;
            Vector2 playerPosition = this.player.transform.position;
            
            float mouseDistance = Vector2.Distance(itemPosition, mousePosition);
            float playerDistance = Vector2.Distance(itemPosition, playerPosition);

            bool canPickup =
                mouseDistance < Game.defaultPickupRange &&
                playerDistance < Game.WeaponPickupRange;
            
            SetHoveredState(canPickup);

            if (!this.hovered)
                return;
            if (Input.GetMouseButtonDown(0))
                OnClick();
        }
        protected abstract void OnClick();
        
        protected abstract void OnHoverStart();
        protected abstract void OnHoverEnd();
    }
}
