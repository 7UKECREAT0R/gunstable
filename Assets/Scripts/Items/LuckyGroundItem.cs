using UI;
using UnityEngine;

namespace Items
{
    public class LuckyGroundItem : GroundItem
    {
        public LuckyObject luckyObject;
        
        private bool clickable = true;
        private bool popupActive;
        private LuckyObjectPopup popup;

        protected override void Start()
        {
            base.Start();
            
            // get sprite
            this.spriteRenderer.sprite = Resources.LoadAll<Sprite>
                ("Sprites/Lucky/" + this.luckyObject.ToString().ToLower())[0];
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
            if (!this.clickable)
                return;
            this.clickable = false;

            CameraEffects effects = CameraEffects.SINGLETON;
            string luckyObjectName = this.luckyObject.ToString().ToUpper();
            effects.CreateActionText(this.transform.position, "+1 " + luckyObjectName, Rarity.GetColorForRarity(RarityType.Unremarkable), 0.5F, 3F);

            OnHoverEnd();
            Game.Collect(this.luckyObject);
            Destroy(this.gameObject);
        }
        
        protected override void OnHoverStart()
        {
            if (this.popupActive)
                return;
            
            CameraEffects effects = CameraEffects.SINGLETON;
            Vector2 location = this.cam.ScreenToWorldPoint(Input.mousePosition);
            
            this.popupActive = true;
            this.popup = Instantiate(effects.luckyItemPopupPrefab, location, effects.luckyItemPopupPrefab.transform.rotation)
                .GetComponent<LuckyObjectPopup>();
            this.popup.SetLuckyObject(this.luckyObject);
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