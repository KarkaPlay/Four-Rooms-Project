using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    
    public float CrouchedSpeed = 2;
    public float speed = 5;

    public float targetMovingSpeed = 5;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Crouch.CrouchedForSpeed)
            targetMovingSpeed = CrouchedSpeed;
        else
            targetMovingSpeed = speed;
        
        Vector2 targetVelocity = new Vector2( Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);
        playerRb.velocity = transform.rotation * new Vector3(targetVelocity.x, playerRb.velocity.y, targetVelocity.y);
    }
}
