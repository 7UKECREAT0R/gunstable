using System;
using System.Collections;
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
        public TMP_Text infoText;
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

        private const float INFO_PER_CHAR_DELAY = 0.02F;
        private Coroutine infoStringCoroutine;
        public void DisplayInfoString(string text, float duration, Color color)
        {
            if(this.infoStringCoroutine != null)
                StopCoroutine(this.infoStringCoroutine);
            this.infoText.color = color;
            this.infoStringCoroutine = StartCoroutine(InfoStringAsync(text, duration));
        }
        private IEnumerator InfoStringAsync(string text, float duration)
        {
            for (int i = 0; i <= text.Length; i++)
            {
                string substr = text[..i];
                this.infoText.text = substr;
                yield return new WaitForSeconds(INFO_PER_CHAR_DELAY);
            }

            yield return new WaitForSeconds(duration);
            
            for (int i = text.Length; i > 0; i--)
            {
                string substr = text[..i];
                this.infoText.text = substr;
                yield return new WaitForSeconds(INFO_PER_CHAR_DELAY);
            }

            this.infoText.text = string.Empty;
            this.infoStringCoroutine = null;
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
