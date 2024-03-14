using UI;
using UnityEngine;

namespace Items
{
    public class LuckyItem : GroundItem
    {
        public LuckyObject luckyObject;
        
        private bool clickable = true;
        private bool popupActive;
        private LuckyObjectPopup popup;
        private LuckyUIDriver ui;

        protected override void Start()
        {
            base.Start();
            
            // get ui
            this.ui = FindObjectOfType<LuckyUIDriver>();
            
            // get sprite
            this.spriteRenderer.sprite = Resources.LoadAll<Sprite>
                ("Sprites/Lucky/" + this.luckyObject.ToString().ToLower())[0];
            SetRarityType(RarityType.Unremarkable);
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

            GlobalStuff effects = GlobalStuff.SINGLETON;
            string luckyObjectName = this.luckyObject.ToString().ToUpper();
            effects.CreateActionText(this.transform.position, "+1 " + luckyObjectName, 
                RarityType.Unremarkable.GetColor() * 0.75F, 
                RarityType.Unremarkable.GetColor() * 0.5F,
                0.5F, 3F);
            OnHoverEnd();
            Game.Collect(this.luckyObject);
            this.ui.UpdateTexts();
            Destroy(this.gameObject);
        }
        
        protected override void OnHoverStart()
        {
            if (this.popupActive)
                return;
            
            GlobalStuff effects = GlobalStuff.SINGLETON;
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