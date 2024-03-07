using TMPro;
using UnityEngine;

namespace UI
{
    public class TextWiggle : MonoBehaviour
    {
        private const float WIGGLE_LERP = 15F;
        private const float WIGGLE_AMOUNT = 2F;

        public string Text
        {
            set
            {
                string currentText = this.text.text;
                if (currentText.Equals(value))
                    return;
                Wiggle();
                this.text.text = value;
            }
        }

        private TMP_Text text;
        private RectTransform selfTransform;
        private Vector2 basePosition;

        private void Start()
        {
            this.text = GetComponent<TMP_Text>();
            this.selfTransform = GetComponent<RectTransform>();
            this.basePosition = this.selfTransform.anchoredPosition;
        }
        private void Update()
        {
            Vector2 currentPosition = this.selfTransform.anchoredPosition;
            Vector2 newPosition = Vector2.Lerp(currentPosition, this.basePosition, WIGGLE_LERP * Time.deltaTime);
            this.selfTransform.anchoredPosition = newPosition;
        }
        
        /// <summary>
        /// Wiggles this text.
        /// </summary>
        private void Wiggle()
        {
            Vector2 newPosition = this.basePosition + Vector2.down * WIGGLE_AMOUNT;
            this.selfTransform.anchoredPosition = newPosition;
        }
    }
}