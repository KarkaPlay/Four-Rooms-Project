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

    public bool On_The_Minus;      //Если надо поворачивать в -

    private void Start()
    {
        if(On_The_Minus)
            openAngle *=-1;

        if (open.y == 1 || close.y == 1){
            parentRotation = transform.parent.rotation.eulerAngles.y;
            open.y = 180 + openAngle + parentRotation;
            close.y = 180 + parentRotation;

            open.x = transform.rotation.eulerAngles.x;  //Это на случай, если начальное х или z равно не нулю. Чтобы оно так и оставалось начальным
            open.z = transform.rotation.eulerAngles.z;

            close.x = transform.rotation.eulerAngles.x; //смотри коммент на 27
            close.z = transform.rotation.eulerAngles.z;
        } else{
            if (open.z == 1 || close.z == 1){
            parentRotation = transform.parent.rotation.eulerAngles.z;
            close.z = transform.rotation.eulerAngles.z;
            open.z = openAngle + parentRotation;

            open.x = transform.rotation.eulerAngles.x;  //смотри коммент на 27 (Принцип тот же. Только координаты другие)
            open.y = transform.rotation.eulerAngles.y;  

            close.x = transform.rotation.eulerAngles.x; //смотри коммент на 27 (Принцип тот же. Только координаты другие)
            close.y = transform.rotation.eulerAngles.y; 
            } else {
                if (open.x == 1 || close.x == 1){
                    parentRotation = transform.parent.rotation.eulerAngles.x;
                    close.x = transform.rotation.eulerAngles.x;
                    open.x = openAngle + parentRotation;

                    open.y = transform.rotation.eulerAngles.y;  //смотри коммент на 27 (Принцип тот же. Только координаты другие)
                    open.z = transform.rotation.eulerAngles.z; 

                    close.y = transform.rotation.eulerAngles.y; //смотри коммент на 27 (Принцип тот же. Только координаты другие)
                    close.z = transform.rotation.eulerAngles.z; 
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
