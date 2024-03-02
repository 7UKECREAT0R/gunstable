using TMPro;
using UnityEngine;

namespace UI
{
    public class GunPopup : Popup
    {
        public TMP_Text gunNameDisplay;
        public TMP_Text damageDisplay;
        public TMP_Text cooldownDisplay;
        public TMP_Text accuracyDisplay;

        /// <summary>
        /// Set the full attributes of this GunPopup.
        /// </summary>
        /// <param name="gun">The gun to display.</param>
        public void SetGun(Gun gun)
        {
            SetColor(gun.rarity);
            this.GunName = gun.name;
            this.Damage = gun.damage;
            this.CooldownSeconds = gun.cooldown;
            this.AccuracyPercent = Mathf.RoundToInt((1.0f - gun.inaccuracy) * 100);
        }
        private void SetColor(RarityType rarity) =>
            SetColor(Rarity.GetColorForRarity(rarity));
        private void SetColor(Color color)
        {
            this.gunNameDisplay.color = color;
            this.damageDisplay.color = color;
            this.cooldownDisplay.color = color;
            this.accuracyDisplay.color = color;
        }

        public string GunName
        {
            set => this.gunNameDisplay.text = value;
        }
        public int Damage
        {
            set => this.gunNameDisplay.text = value.ToString();
        }
        public float CooldownSeconds
        {
            set => this.gunNameDisplay.text = value + " SECONDS";
        }
        public int AccuracyPercent
        {
            set => this.gunNameDisplay.text = value + "%";
        }
    }
}