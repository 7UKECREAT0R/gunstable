using UI;
using UnityEngine;

namespace Items
{
    public class GunItem : GroundItem
    {
        public Gun gun;
        
        private bool clickable = true;
        private bool popupActive;
        private GunPopup popup;
        
        protected override void Start()
        {
            base.Start();
            
            // get sprite
            this.spriteRenderer.sprite = this.gun.LoadSprite();
            SetRarityType(this.gun.rarity);
        }
        protected override void Update()
        {
            base.Update();

            if (!this.popupActive)
                return;
            
            Vector2 location = this.cam.ScreenToWorldPoint(Input.mousePosition);
            this.popup.transform.position = location;
        }
        protected override void OnClick()
        {
            if (this.player.HasGun)
                return;
            if (!this.clickable)
                return;
            this.clickable = false;
            
            OnHoverEnd();
            this.player.Gun = this.gun;
            Destroy(this.gameObject);
        }
        
        protected override void OnHoverStart()
        {
            if (this.popupActive)
                return;
            
            GlobalStuff effects = GlobalStuff.SINGLETON;
            effects.PlaySound(GlobalStuff.SoundEffect.UIHover);
            Vector2 location = this.cam.ScreenToWorldPoint(Input.mousePosition);
            
            this.popupActive = true;
            this.popup = Instantiate(effects.gunPopupPrefab, location,
                    effects.gunPopupPrefab.transform.rotation)
                .GetComponent<GunPopup>();
            this.popup.SetGun(this.gun); 
        }
        protected override void OnHoverEnd()
        {
            if (!this.popupActive)
                return;
            
            Destroy(this.popup.gameObject);
            this.popupActive = false;
        }
    }
}