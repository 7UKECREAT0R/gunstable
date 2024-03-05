using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ActionText : MonoBehaviour
{
    private const float SWITCH_SPEED = 0.05F;
    
    private string textContent;
    private Color? textColorA;
    private Color? textColorB;
    
    private float switchTimer;
    private bool useB;
    
    private TMP_Text text;
    public float lifetime;
    public float speed;

    public void RunWith(string text, Color colorA, Color colorB, float lifetime, float speed = 1.0f)
    {
        this.lifetime = lifetime;
        this.speed = speed;

        if (this.text == null)
        {
            this.textContent = text;
            this.textColorA = colorA;
            this.textColorB = colorB;
        }
        else
        {
            this.text.text = text;
            this.textColorA = colorA;
            this.textColorB = colorB;
        }
    }
    private void Start()
    {
        this.text = GetComponent<TMP_Text>();

        if (this.textContent != null)
            this.text.text = this.textContent;

        if (this.textColorA.HasValue)
            this.text.color = this.textColorA.Value;
    }
    private void Update()
    {
        float dt = Time.deltaTime;

        this.switchTimer -= dt;
        if (this.switchTimer < 0F)
        {
            this.switchTimer = SWITCH_SPEED;
            this.useB = !this.useB;
            this.text.color = this.useB ?
                this.textColorB.GetValueOrDefault() :
                this.textColorA.GetValueOrDefault();
        }
        
        this.lifetime -= dt;
        if (this.lifetime < 0F)
        {
            Destroy(this.gameObject);
            return;
        }
        
        float speedCoefficient = Mathf.Pow(Mathf.Clamp(this.lifetime, 0.0f, 1.0f), 2.0f);
        Vector2 movement = Vector2.up * (speedCoefficient * this.speed * dt);
        this.transform.Translate(movement);
    }
}