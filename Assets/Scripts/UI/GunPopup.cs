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
            this.Damage = gun.Damage;
            this.CooldownSeconds = gun.Cooldown;

            float normalizedInaccuracy = Mathf.Abs(gun.inaccuracy) / 3.6F;
            this.AccuracyPercent = Mathf.RoundToInt(100F - normalizedInaccuracy);
        }
        private void SetColor(RarityType rarity) =>
            SetColor(rarity.GetColor());
        private void SetColor(Color color)
        {
            this.gunNameDisplay.color = color;
            Color others = Color.white;
            this.damageDisplay.color = others;
            this.cooldownDisplay.color = others;
            this.accuracyDisplay.color = others;
        }

        public string GunName
        {
            set => this.gunNameDisplay.text = value;
        }
        public int Damage
        {
            set => this.damageDisplay.text = value.ToString();
        }
        public float CooldownSeconds
        {
            set => this.cooldownDisplay.text = value + " SECONDS";
        }
        public int AccuracyPercent
        {
            set => this.accuracyDisplay.text = value + "%";
        }
    }
}