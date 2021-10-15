using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMoving : MonoBehaviour
{
    [SerializeField] private Vector3 targetPos = new Vector3(150, 50, 0);
    private float step;

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        step = Time.deltaTime * 100.0f;
        targetPos.x = (transform.childCount - 1) * 50 + Screen.width/2;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
    }
}
