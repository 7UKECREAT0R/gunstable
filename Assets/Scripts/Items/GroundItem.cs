using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class GroundItem : MonoBehaviour
    {
        private bool hovered;

        protected SpriteRenderer spriteRenderer;
        protected Camera cam;
        
        private RarityType type = RarityType.Unremarkable;
        private Player player;

        public void SetRarityType(RarityType type)
        {
            if (type != this.type)
                SetMaterial(this.hovered);
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
            CameraEffects effects = CameraEffects.SINGLETON;
            
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
        }
        protected virtual void Update()
        {
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
        
        // ReSharper disable Unity.PerformanceAnalysis
        protected abstract void OnHoverStart();
        // ReSharper disable Unity.PerformanceAnalysis
        protected abstract void OnHoverEnd();
    }
}
