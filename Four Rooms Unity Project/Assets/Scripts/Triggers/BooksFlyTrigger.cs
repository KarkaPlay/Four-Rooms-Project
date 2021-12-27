using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BooksFlyTrigger : MonoBehaviour
{
    public GameObject books;
    public GameObject fire;
    private Rigidbody bookRb;
    private float speed;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        fire.gameObject.SetActive(true);
        for (int i = 0; i < books.transform.childCount; i++)
        {
            speed = Random.Range(1.0f, 7.0f);
            bookRb = books.transform.GetChild(i).GetComponent<Rigidbody>();
            bookRb.AddForce(Vector3.up*speed, ForceMode.Acceleration);
            bookRb.AddTorque(new Vector3(Random.Range(0, 90), Random.Range(0, 90), Random.Range(0, 90)));
        }
    }
}
