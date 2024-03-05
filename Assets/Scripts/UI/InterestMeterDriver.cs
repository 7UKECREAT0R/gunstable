using UnityEngine;

namespace UI
{
    public class InterestMeterDriver : MonoBehaviour
    {
        private RectTransform maskTransform;
        public RectTransform childTransform;

        private const int PIXELS = 57;
        private const float EMPTY_X = -16.5F;
        private const float CHILD_MIDPOINT_X = 31.5F;
    
        private int PixelsFull
        {
            set
            {
                if (value < 0)
                    value = 0;
                if (value > PIXELS)
                    value = PIXELS;
                
                float maskOffset = EMPTY_X + value;
                float inverseOffset = CHILD_MIDPOINT_X + (PIXELS - value);
                Debug.Log($"{value} Pixels. Mask Offset: {maskOffset}, Child Offset: {inverseOffset}");
                
                Vector3 maskPosition = this.maskTransform.anchoredPosition;
                Vector3 childPosition = this.childTransform.anchoredPosition;
                maskPosition.x = maskOffset;
                childPosition.x = inverseOffset;
                this.maskTransform.anchoredPosition = maskPosition;
                this.childTransform.anchoredPosition = childPosition;
                this.maskTransform.ForceUpdateRectTransforms();
                this.childTransform.ForceUpdateRectTransforms();
            }
        }
        /// <summary>
        /// The amount this bar should be full, from 0-1.
        /// </summary>
        public float AmountFull
        {
            set
            {
                float adjustedFloat = value * PIXELS;
                this.PixelsFull = Mathf.RoundToInt(adjustedFloat);
            }
        }

        private void Start()
        {
            this.maskTransform = GetComponent<RectTransform>();
        }
    }
}
