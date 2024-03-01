using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Character
{
    public float movementSpeed = 1.0f;
    
    private Animator animator;
    private bool isMoving;
    
    private LookDirection lastDirection;
    private bool lastIsMoving;

    /// <summary>
    /// Look towards the given direction. Returns the un-normalized direction from the player to the given location.
    /// </summary>
    /// <param name="location">The location to look at.</param>
    /// <returns>Direction from the player to the given location, not normalized.</returns>
    public Vector2 LookTowards(Vector2 location)
    {
        Vector2 pos = location - (Vector2)this.transform.position;
        Vector2 diff = pos.normalized;
        this.direction = FromDirection(diff);
        
        if (this.direction != this.lastDirection)
            UpdateAnimator();

        this.lastDirection = this.direction;
        return pos;
    }
    private void UpdateAnimator()
    {
        string stateName = this.isMoving ?
            "playerMove" + this.direction :
            "playerStand" + this.direction;
        this.animator.Play(stateName);
    }
    
    
    public override void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public override void AfterDamage(int damageAmount, bool died)
    {
        CameraEffects effects = CameraEffects.SINGLETON;
        effects.StartShake(new Shake(1.0f, 30.0f, 0.5f));
    }

    public override void Start()
    {
        base.Start();
        this.animator = GetComponent<Animator>();
    }
    private void Update()
    {
        float deltaTime = Time.deltaTime;

        Vector2 movementVector = Vector2.zero;
        this.isMoving = false;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            movementVector += Vector2.left;
            this.isMoving = true;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            movementVector += Vector2.right;
            this.isMoving = true;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            movementVector += Vector2.up;
            this.isMoving = true;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            movementVector += Vector2.down;
            this.isMoving = true;
        }

        if (this.isMoving)
        {
            movementVector.Normalize();
            movementVector *= this.movementSpeed;
            movementVector *= deltaTime;
            this.transform.Translate(movementVector);
        }
        
        if (this.isMoving != this.lastIsMoving)
            UpdateAnimator();
        
        this.lastIsMoving = this.isMoving;
    }
}