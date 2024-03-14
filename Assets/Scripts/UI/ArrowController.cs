using System;
using UnityEngine;

namespace UI
{
    public class ArrowController : MonoBehaviour
    {
        private GameObject arrowChild;
        
        /// <summary>
        /// The angle the arrow points in.
        /// </summary>
        public float angle;
        /// <summary>
        /// The location the arrow points towards.
        /// </summary>
        public Vector2 Facing
        {
            set => this.angle = Vector2.SignedAngle(Vector2.right, value);
        }

        private bool _enabled;
        /// <summary>
        /// If the arrow is visibly enabled.
        /// </summary>
        public bool Enabled
        {
            get => this._enabled;
            set
            {
                if (value == this._enabled)
                    return;
                this._enabled = value;
                this.arrowChild.SetActive(value);
            }
        }

        private void Start()
        {
            this.arrowChild = this.transform.GetChild(0).gameObject;
        }
        private void Update()
        {
            this.transform.localRotation = Quaternion.Euler(0, 0, this.angle);
        }
    }
}