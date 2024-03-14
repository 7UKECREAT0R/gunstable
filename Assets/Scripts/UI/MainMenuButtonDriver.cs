using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    [RequireComponent(typeof(AudioSource))]
    public class MainMenuButtonDriver : MonoBehaviour
    {
        public ButtonActionType actionType;

        
        private AudioSource audioSource;
        private RectTransform rect;
        private Canvas mainCanvas;
        private RectTransform canvasRect;
        private TMP_Text text;
        private bool hovered;

        private static readonly Color textColorNormal = new Color(0.18F, 0.34F, 0.33F);
        private static readonly Color textColorHover = new Color(0.57F, 0.91F, 0.75F);
        
        public void PlaySound(GlobalStuff.SoundEffect effect)
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/" + effect);
            this.audioSource.PlayOneShot(clip);
        }
        private void Start()
        {
            this.audioSource = GetComponent<AudioSource>();
            this.mainCanvas = GetComponentInParent<Canvas>();
            this.canvasRect = this.mainCanvas.GetComponent<RectTransform>();
            
            this.rect = GetComponent<RectTransform>();
            this.text = GetComponentInChildren<TMP_Text>();
        }

        /// <summary>
        /// Returns the canvas-space Rect that this transform takes up.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to get the Rect for.</param>
        private static Rect GetCanvasSpaceRect(RectTransform rectTransform)
        {
            Vector2 anchorMin = rectTransform.anchorMin;
            Vector2 anchorMax = rectTransform.anchorMax;
            Vector2 anchorSize = rectTransform.sizeDelta;

            Vector2 absoluteMin = (anchorMin + rectTransform.anchoredPosition - anchorSize * rectTransform.pivot);
            Vector2 absoluteSize = anchorSize * ((anchorMax - anchorMin) + Vector2.one);

            return new Rect(absoluteMin, absoluteSize);
        }
        private void Update()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                this.canvasRect,
                Input.mousePosition,
                null,
                out Vector2 mousePosition);

            Rect buttonRect = GetCanvasSpaceRect(this.rect);
            
            if (buttonRect.Contains(mousePosition))
            {
                if(!this.hovered)
                    OnPointerEnter();
            }
            else {
                if(this.hovered)
                    OnPointerExit();
            }
            
            if (Input.GetMouseButtonDown(0) && this.hovered)
            {
                OnPointerClick();
            }
        }

        private void OnPointerEnter()
        {
            PlaySound(GlobalStuff.SoundEffect.UIHover);
            this.hovered = true;
            this.text.color = textColorHover;
        }
        private void OnPointerExit()
        {
            PlaySound(GlobalStuff.SoundEffect.UIHover);
            this.hovered = false;
            this.text.color = textColorNormal;
        }

        private void OnPointerClick()
        {
            PlaySound(GlobalStuff.SoundEffect.UIClick);

            switch (this.actionType)
            {
                case ButtonActionType.StartGame:
                {
                    Game.Reset();
                    SceneManager.LoadScene("Game");
                    break;
                }
                case ButtonActionType.Help:
                {
                    SceneManager.LoadScene("HelpMenu");
                    break;
                }
                case ButtonActionType.Return:
                {
                    SceneManager.LoadScene("MainMenu");
                    break;
                }
                case ButtonActionType.Exit:
                {
                    Application.Quit();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    public enum ButtonActionType
    
    {
        StartGame,
        Help,
        Return,
        Exit
    }
}