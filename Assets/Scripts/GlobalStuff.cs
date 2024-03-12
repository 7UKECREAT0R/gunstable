using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Camera))]
public class GlobalStuff : MonoBehaviour
{
    public static GlobalStuff SINGLETON;
    private UIDriver ui;
    
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public GameObject bloodParticlePrefab;
    public GameObject shellParticlePrefab;
    public GameObject gunPopupPrefab;
    public GameObject luckyItemPopupPrefab;
    public GameObject actionTextPrefab;
    public Material selectionMaterial;
    public Material[] rarityMaterials;
    public Material hitFlashOther;
    public Material hitFlashPlayer;
    public Material spriteLitDefault;
    
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

    private const float cameraVelocityDrag = 10F;
    private float cameraVelocityX;
    private float cameraVelocityY;
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
    private void ImpulseCamera(float x, float y)
    {
        this.cameraVelocityX += x;
        this.cameraVelocityY += y;
    }
    public void ImpulseCamera(Vector2 velocity) => ImpulseCamera(velocity.x, velocity.y);
    
    public void CreateActionText(Vector2 position, string text, Color colorA, Color colorB, float duration, float speed = 1.0f)
    {
        GameObject spawnedActionText = Instantiate(this.actionTextPrefab);
        spawnedActionText.transform.position = position;
        ActionText actionText = spawnedActionText.GetComponent<ActionText>();
        actionText.RunWith(text, colorA, colorB, duration, speed);
    }
    public void CreateShellParticle(Vector2 position)
    {
        GameObject newParticle = Instantiate(this.shellParticlePrefab);
        newParticle.transform.position = position;
    }

    private readonly Dictionary<int, Enemy> enemies = new();
    public readonly Dictionary<int, GroundItem> droppedItems = new();
    private void UpdateEnemyUIText()
    {
        int count = this.enemies.Count;
        this.ui.EnemiesLeft = count;
    }
    public void SpawnEnemy(Vector2 position, int health, float angle, Enemy.SpriteSheet spriteSheet)
    {
        GameObject enemyObject = Instantiate(this.enemyPrefab);
        enemyObject.transform.position = position;

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemy.GunPointAngle = angle;
        enemy.health = health;
        enemy.spriteSheet = spriteSheet;
        enemy.spriteVariant = (Enemy.SpriteVariant)Random.Range(0, 3);
        enemy.Gun = Game.RollGun();

        this.enemies[enemy.GetInstanceID()] = enemy;
        UpdateEnemyUIText();
    }
    public void RemoveEnemy(Enemy enemy)
    {
        this.enemies.Remove(enemy.GetInstanceID());
        UpdateEnemyUIText();
    }
    public int Enemies => this.enemies.Count;
    private IEnumerable<Enemy> GetAllEnemiesSnapshot()
    {
        return this.enemies.Values.ToArray();
    }
    private IEnumerable<GroundItem> GetAllItemsSnapshot()
    {
        return this.droppedItems.Values.ToArray();
    }
    public void ClearWorld()
    {
        IEnumerable<Enemy> allEnemies = GetAllEnemiesSnapshot();
        IEnumerable<GroundItem> allItems = GetAllItemsSnapshot();

        foreach (Enemy enemy in allEnemies)
            Destroy(enemy.gameObject);
        foreach (GroundItem item in allItems)
            Destroy(item.gameObject);
        
        this.enemies.Clear();
        this.droppedItems.Clear();
    }
    
    private void Start()
    {
        SINGLETON = this;
        this.ui = FindObjectOfType<UIDriver>();
    }
    private void Update()
    {
        float deltaTime = Time.deltaTime;
        Transform change = this.transform;
        Vector3 position = change.localPosition;
        
        if (this.cameraShakes.Count != 0)
        {
            Vector2 shake = Vector2.zero;
            this.cameraShakes.RemoveAll(s => s.Tick(deltaTime, ref shake));
            position += new Vector3(shake.x, shake.y, 0);
        }
        
        // do stuff with camera velocity
        float dragCoefficient = cameraVelocityDrag * deltaTime;
        this.cameraVelocityX = Mathf.Lerp(this.cameraVelocityX, 0F, dragCoefficient);
        this.cameraVelocityY = Mathf.Lerp(this.cameraVelocityY, 0F, dragCoefficient);
        position.x += this.cameraVelocityX * deltaTime;
        position.y += this.cameraVelocityY * deltaTime;
        
        // apply change
        change.localPosition = position;
    }

    private bool isInBulletTime = false;

    public void ActivateBulletTime(float lengthMultiplier)
    {
        if(this.isInBulletTime)
            StopAllCoroutines();
        StartCoroutine(DoBulletTime(Game.BulletTimePerKill * lengthMultiplier));
    }
    private IEnumerator DoBulletTime(float amount)
    {
        const float TRANSITION_TIME = 0.1F;
        this.isInBulletTime = true;

        yield return TransitionFrom(1.0F, Game.bulletTimeSlowness);
        Time.timeScale = Game.bulletTimeSlowness;

        yield return new WaitForSecondsRealtime(amount - TRANSITION_TIME * 2F);
        
        yield return TransitionFrom(Game.bulletTimeSlowness, 1.0F);
        Time.timeScale = 1.0F;
        this.isInBulletTime = false;
        
        yield break;

        IEnumerator TransitionFrom(float timeA, float timeB)
        {
            float time = 0F;
            while (time < TRANSITION_TIME)
            {
                float t = Mathf.Clamp01(time / TRANSITION_TIME);
                Time.timeScale = Mathf.Lerp(timeA, timeB, t);
                yield return new WaitForEndOfFrame();
                time += Time.unscaledDeltaTime;
            }
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