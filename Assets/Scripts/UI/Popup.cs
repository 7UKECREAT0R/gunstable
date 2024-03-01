using System;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Handles general popup stuff like animation.
    /// </summary>
    public class Popup : MonoBehaviour
    {
        private const float animationLength = 0.15F;
        private float animationTimer;

        protected virtual void Start()
        {
            this.transform.localScale = Vector3.zero;
        }

        protected virtual void Update()
        {
            // scale animation when coming in.
            if (this.animationTimer > animationLength)
            {
                this.transform.localScale = Vector3.one;
                return;
            }

            this.animationTimer += Time.deltaTime;
            float animationCompletion = this.animationTimer / animationLength;
            float scaleFactor = (float)System.Math.Cbrt(animationCompletion);
            this.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }
    }
}