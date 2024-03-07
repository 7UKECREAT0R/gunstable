using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIDriver : MonoBehaviour
    {
        public InterestMeterDriver interestMeter;
        public LuckyUIDriver luckyObjects;
        public TextWiggle enemiesLeftText;
        
        private Image thisImage;
        public Sprite[] hpSprites;

        /// <summary>
        /// Sets the number of things shown on the health display.
        /// </summary>
        public int HealthDisplay
        {
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 5)
                    value = 5;

                this.thisImage.sprite = this.hpSprites[value];
            }
        }

        public int EnemiesLeft
        {
            set
            {
                this.enemiesLeftText.Text = value switch
                {
                    < 1 => "00",
                    < 10 => "0" + value,
                    < 100 => "" + value,
                    _ => value.ToString()[..2]
                };
            }
        }
        /// <summary>
        /// The amount this bar should be full, from 0-1.
        /// </summary>
        public float InterestPercent
        {
            set => this.interestMeter.AmountFull = value;
        }

        private void Start()
        {
            this.thisImage = GetComponent<Image>();
        }
    }
}
