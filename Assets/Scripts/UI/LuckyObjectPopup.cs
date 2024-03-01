using TMPro;
using UnityEngine;

namespace UI
{
    public class LuckyObjectPopup : Popup
    {
        public TMP_Text itemNameDisplay;
        public TMP_Text itemDescriptionDisplay;

        public void SetLuckyObject(LuckyObject luckyObject)
        {
            this.ItemName = luckyObject.GetName();
            this.ItemDescription = luckyObject.GetDescription();
        }
        
        public string ItemName
        {
            set => this.itemNameDisplay.text = value;
        }
        public string ItemDescription
        {
            set => this.itemDescriptionDisplay.text = value;
        }
    }
}