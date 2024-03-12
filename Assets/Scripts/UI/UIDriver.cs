using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class UIDriver : MonoBehaviour
    {
        public GameObject pauseMenu;
        public InterestMeterDriver interestMeter;
        public LuckyUIDriver luckyObjects;
        public TextWiggle enemiesLeftText;
        public bool shaking;

        private const float SHAKE_MAGNITUDE = 1.5F;
        private const float SHAKE_FREQUENCY = 0.1F;
        private float shakeCooldown;
        
        private Vector2 basePosition;
        private RectTransform rect;
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
        /// <summary>
        /// Sets if the pause menu should be visible.
        /// </summary>
        public bool PauseMenuIsVisible
        {
            set => this.pauseMenu.SetActive(value);
        }
        
        private void Update()
        {
            if (!this.shaking)
            {
                this.rect.anchoredPosition = this.basePosition;
                return;
            }

            float deltaTime = Time.deltaTime;
            this.shakeCooldown -= deltaTime;
            
            if (this.shakeCooldown > 0F)
                return;
            
            Vector2 newPosition = this.basePosition;
            newPosition.x += Random.Range(-SHAKE_MAGNITUDE, SHAKE_MAGNITUDE);
            newPosition.y += Random.Range(-SHAKE_MAGNITUDE, SHAKE_MAGNITUDE);
            this.rect.anchoredPosition = newPosition;
        }
        private void Start()
        {
            this.rect = GetComponent<RectTransform>();
            this.basePosition = this.rect.anchoredPosition;
            
            this.thisImage = GetComponent<Image>();
        }
    }
}
