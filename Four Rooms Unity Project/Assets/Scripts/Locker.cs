using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locker : MonoBehaviour
{
    private float openSpeed = 3.0f;
    private Vector3 open, close;
    public Vector3 openingDirection;
    public float openingLength;
    public bool isOpen;
    public bool needKnife;

    private void Start()
    {
        close = transform.position;
        open = close - transform.TransformDirection(openingDirection) * openingLength;
    }

    private void FixedUpdate()
    {
        if ((transform.position == open && isOpen) || (transform.position == close && !isOpen)) return;
        Use(isOpen ? open : close);
    }

    void Use(Vector3 state)
    {
        transform.position = Vector3.MoveTowards(transform.position, state, openSpeed * Time.deltaTime);
    }
}
