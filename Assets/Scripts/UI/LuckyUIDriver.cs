using System;
using System.Collections;
using System.Diagnostics.Contracts;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LuckyUIDriver : MonoBehaviour
    {
        public TextWiggle cloversText;
        public TextWiggle ringsText;
        public TextWiggle rocksText;
        public TextWiggle sandbagsText;
        public TextWiggle springsText;

        [Pure]
        private static string Stringify(int number)
        {
            if (number < 1)
                return "0";
            return number > 9 ? "+" : number.ToString();
        }
        
        public int Clovers
        {
            set => this.cloversText.Text = Stringify(value);
        }
        public int Rings
        {
            set => this.ringsText.Text = Stringify(value);
        }
        public int Rocks
        {
            set => this.rocksText.Text = Stringify(value);
        }
        public int Sandbags
        {
            set => this.sandbagsText.Text = Stringify(value);
        }
        public int Springs
        {
            set => this.springsText.Text = Stringify(value);
        }

        private void Start()
        {
            UpdateTexts();
        }
        public void UpdateTexts()
        {
            StartCoroutine(UpdateTextsLater());
        }
        private IEnumerator UpdateTextsLater()
        {
            yield return new WaitForFixedUpdate();
            this.Clovers = Game.GetCount(LuckyObject.Clover);
            this.Rings = Game.GetCount(LuckyObject.Ring);
            this.Rocks = Game.GetCount(LuckyObject.Rock);
            this.Sandbags = Game.GetCount(LuckyObject.Sandbag);
            this.Springs = Game.GetCount(LuckyObject.Spring);
        }
    }
}