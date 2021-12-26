using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private float rotationSpeed = 60.0f;
    [InspectorName("Rotation Axis")]public Vector3 open;
    private Vector3 close;
    [Range(0, 180)]public float openAngle = 120;
    [HideInInspector]public bool isOpen;
    private Quaternion openQ, closeQ;

    public bool needKey;
    public bool rotateBackwards; //Если надо поворачивать в -

    private void Start()
    {
        if (rotateBackwards)
            openAngle *= -1;

        var rotation = transform.rotation;
        close = rotation.eulerAngles;
        open = rotation.eulerAngles + Vector3.Scale(open, Vector3.one * openAngle);
        
        openQ = Quaternion.Euler(open);
        closeQ = Quaternion.Euler(close);
    }
    
    private void FixedUpdate()
    {
        if ((transform.rotation == openQ && isOpen) || (transform.rotation == closeQ && !isOpen)) return;
        Use(isOpen ? openQ : closeQ);
    }

    public void Use(Quaternion state)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, state, rotationSpeed * Time.deltaTime);
    }
}