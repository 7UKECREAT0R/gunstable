using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class MainMenuLogoDriver : MonoBehaviour
    {
        private static readonly char[] EASTER_EGG_CHARS = "yucky".ToCharArray();
        
        private int easterEggProgress;
        private void EnterEasterEggChar(char c)
        {
            char nextNeeded = this.easterEggProgress >= EASTER_EGG_CHARS.Length ?
                '\0' :
                EASTER_EGG_CHARS[this.easterEggProgress];

            if (c == nextNeeded)
            {
                this.easterEggProgress++;
                if (this.easterEggProgress >= EASTER_EGG_CHARS.Length)
                    this.UseAlternateLogo = true;
            }
            else
            {
                this.easterEggProgress = 0;
                this.UseAlternateLogo = false;
            }
        }
        
        private const float MIDDLE_X = -120F;
        private const float MIDDLE_Y = 67.5F;
        
        private RectTransform rectTransform;
        private Image imageRenderer;
        
        public Sprite regularLogo;
        public Sprite alternateLogo;

        public bool UseAlternateLogo
        {
            set
            {
                Sprite newSprite = value ? this.alternateLogo : this.regularLogo;
                this.imageRenderer.sprite = newSprite;
                this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSprite.rect.width);
                this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSprite.rect.height);
            }
        }

        private void Start()
        {
            this.rectTransform = GetComponent<RectTransform>();
            this.imageRenderer = GetComponent<Image>();
        }
        private void Update()
        {
            string inputString = Input.inputString;
            if (!string.IsNullOrEmpty(inputString))
            {
                char[] enteredChars = inputString.ToCharArray();
                foreach (char enteredChar in enteredChars)
                    EnterEasterEggChar(enteredChar);
            }
        }
    }
}
