using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    public static CameraEffects SINGLETON;

    public GameObject shellParticlePrefab;
    public GameObject gunPopupPrefab;
    public GameObject luckyItemPopupPrefab;
    public GameObject actionTextPrefab;
    public Material selectionMaterial;
    public Material[] rarityMaterials;
    /// <summary>
    /// Returns the material for the given rarity. Will outline the
    /// sprite with the color associated with the rarity.
    /// </summary>
    /// <param name="rarity">The rarity to get the material for.</param>
    /// <returns>The material.</returns>
    public Material GetMaterialForRarity(RarityType rarity)
    {
        return this.rarityMaterials[(int)rarity];
    }
    
    private readonly List<Shake> cameraShakes = new();
    /// <summary>
    /// Begins the given <see cref="Shake"/>.
    /// </summary>
    /// <param name="shake"></param>
    public void StartShake(Shake shake)
    {
        Debug.Assert(shake != null, "Shake was null.");
        this.cameraShakes.Add(shake);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void CreateActionText(Vector2 position, string text, Color color, float duration, float speed = 1.0f)
    {
        GameObject spawnedActionText = Instantiate(this.actionTextPrefab);
        spawnedActionText.transform.position = position;
        ActionText actionText = spawnedActionText.GetComponent<ActionText>();
        actionText.RunWith(text, color, duration, speed);
    }
    // ReSharper disable Unity.PerformanceAnalysis
    public void CreateShellParticle(Vector2 position)
    {
        GameObject newParticle = Instantiate(this.shellParticlePrefab);
        newParticle.transform.position = position;
    }
    
    private void Start()
    {
        SINGLETON = this;
    }
    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (this.cameraShakes.Count != 0)
        {
            Vector2 shake = Vector2.zero;
            this.cameraShakes.RemoveAll(s => s.Tick(deltaTime, ref shake));
            Transform change = this.transform;
            
            Vector3 position = change.localPosition;
            position += new Vector3(shake.x, shake.y, 0);
            change.localPosition = position;
        }
    }
    
    
}

public class Shake
{
    private float currentTime;
    private readonly float magnitude;
    private readonly float frequency;
    private readonly float finishedTime;
    private readonly float perlinSeed;

    /// <summary>
    /// 0.0-1.0 how far this shake is completed.
    /// </summary>
    private float PercentCompleted => this.currentTime / this.finishedTime;

    /// <summary>
    /// Create a new Shake
    /// </summary>
    /// <param name="magnitude">The maximum magnitude of the shake, in pixels. The magnitude scales down over time.</param>
    /// <param name="frequency">The frequency of the shake, i.e. how fast it is.</param>
    /// <param name="duration">The duration of the shake.</param>
    public Shake(float magnitude, float frequency, float duration)
    {
        this.magnitude = magnitude / 100.0f;
        this.frequency = frequency;
        this.currentTime = 0F;
        this.finishedTime = duration;
        this.perlinSeed = Random.value * 1000.0F;
    }

    /// <summary>
    /// Ticks the shake effect
    /// </summary>
    /// <param name="deltaTime">The current deltaTime.</param>
    /// <param name="currentShake">The shake to apply transform to.</param>
    /// <returns>If this Shake is done shaking.</returns>
    public bool Tick(float deltaTime, ref Vector2 currentShake)
    {
        this.currentTime += deltaTime;
        
        if (this.currentTime > this.finishedTime)
            return true;
        
        float adjustedMagnitude = Mathf.Clamp(this.magnitude * (1.0f - this.PercentCompleted), 0.0F, this.magnitude);
        float sample = this.currentTime * this.frequency;
        
        // use perlin noise as shake, adjusted for -1.0 to 1.0 range
        float shakeX = Mathf.PerlinNoise(sample, this.perlinSeed) * 2.0f - 1.0f;
        float shakeY = Mathf.PerlinNoise(this.perlinSeed, sample) * 2.0f - 1.0f;
        
        // add to the shake stack
        currentShake.x += shakeX * adjustedMagnitude;
        currentShake.y += shakeY * adjustedMagnitude;
        return false;
    }
}