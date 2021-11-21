using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private float rotationSpeed = 60.0f;
    public Vector3  open, close;
    public float openAngle = 120;
    public bool isOpen;
    private Quaternion openQ, closeQ; 
    private float parentRotation;

    public bool On_The_Minus;      //Если надо поворачивать в -

    private void Start()
    {
        if(On_The_Minus)
            openAngle *=-1;

        close = transform.rotation.eulerAngles;

        if (open.y == 1){

            parentRotation = transform.parent.rotation.eulerAngles.y;

            open = transform.rotation.eulerAngles + new Vector3(0,  openAngle, 0);
            close = transform.rotation.eulerAngles;
        } else{
            if (open.z == 1){
            parentRotation = transform.parent.rotation.eulerAngles.z;

            open = transform.rotation.eulerAngles + new Vector3(0, 0, openAngle);
            } else {
                if (open.x == 1){
                    parentRotation = transform.parent.rotation.eulerAngles.x;
                    
                    open = transform.rotation.eulerAngles + new Vector3(openAngle,0, 0);
            }
            }
        }
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
