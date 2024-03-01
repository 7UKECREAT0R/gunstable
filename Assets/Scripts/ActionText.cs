using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ActionText : MonoBehaviour
{
    private string textContent;
    private Color? textColor;
    
    private TMP_Text text;
    public float lifetime;
    public float speed;

    public void RunWith(string text, Color color, float lifetime, float speed = 1.0f)
    {
        this.lifetime = lifetime;
        this.speed = speed;

        if (this.text == null)
        {
            this.textContent = text;
            this.textColor = color;
        }
        else
        {
            this.text.text = text;
            this.text.color = color;
        }
    }
    private void Start()
    {
        this.text = GetComponent<TMP_Text>();
        
        if(this.textContent != null)
            this.text.text = this.textContent;
        if(this.textColor.HasValue)
            this.text.color = this.textColor.Value;
    }
    private void Update()
    {
        float dt = Time.deltaTime;
        
        this.lifetime -= dt;
        if (this.lifetime < 0)
        {
            Destroy(this.gameObject);
            return;
        }
        
        float speedCoefficient = Mathf.Pow(Mathf.Clamp(this.lifetime, 0.0f, 1.0f), 2.0f);
        Vector2 movement = Vector2.up * (speedCoefficient * this.speed * dt);
        this.transform.Translate(movement);
    }
}