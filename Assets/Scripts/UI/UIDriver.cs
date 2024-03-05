using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIDriver : MonoBehaviour
    {
        public InterestMeterDriver interestMeter;
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
        /// <summary>
        /// The amount this bar should be full, from 0-1.
        /// </summary>
        public float InterestPercent
        {
            set
            {
                Debug.Log("interest percent being set to: " + value);
                this.interestMeter.AmountFull = value;
            }
        }

        private void Start()
        {
            this.thisImage = GetComponent<Image>();
        }
    }
}
