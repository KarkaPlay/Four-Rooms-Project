using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private float rotationSpeed = 60.0f;
    public Vector3 close, open;
    public float openAngle = 120;
    public bool isOpen;
    private Quaternion openQ, closeQ; 
    private float parentRotation;

    private void Start()
    {
        parentRotation = transform.parent.rotation.eulerAngles.y;
        open.y = 180 + openAngle + parentRotation;
        close.y = 180 + parentRotation;
        
        openQ = Quaternion.Euler(open);
        closeQ = Quaternion.Euler(close);
    }
    
    private void FixedUpdate()
    {
        if ((transform.rotation == openQ && isOpen) || (transform.rotation == closeQ && !isOpen)) return;
        Use(isOpen ? openQ : closeQ);
    }

    void Use(Quaternion state)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, state, rotationSpeed * Time.deltaTime);
    }
}
