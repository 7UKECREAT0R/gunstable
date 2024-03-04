using System;
using Characters;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private const float MOVEMENT_DIVISOR = 2.0f;
    private const float MOVEMENT_LIMIT = 1.0f;
    
    private Camera cam;
    public Player player;
    
    [HideInInspector]
    public Vector2 cursorLocation;

    private void Start()
    {
        this.cam = Camera.main;
    }
    private void Update()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        
        this.cursorLocation = this.cam.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = this.cursorLocation;
        
        Vector2 diff = this.player.LookTowards(this.cursorLocation);
        diff /= MOVEMENT_DIVISOR;
        
        Vector3 cameraPosition = this.cam.transform.localPosition;
        Vector3 targetPosition = cameraPosition + new Vector3(diff.x, diff.y, 0.0F);

        if (targetPosition.sqrMagnitude > MOVEMENT_LIMIT * MOVEMENT_LIMIT)
            targetPosition = targetPosition.normalized * MOVEMENT_LIMIT;

        targetPosition.z = cameraPosition.z;
        this.cam.transform.localPosition = Vector3.Lerp(cameraPosition, targetPosition, 20.0f * Time.deltaTime);
    }
}