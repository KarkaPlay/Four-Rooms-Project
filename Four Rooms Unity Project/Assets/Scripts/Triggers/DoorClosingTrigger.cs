using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorClosingTrigger : MonoBehaviour
{
    public GameObject door;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        door.GetComponent<Door>().isOpen = !door.GetComponent<Door>().isOpen;
        Destroy(door.GetComponent<Collider>());
        Destroy(gameObject);
    }
}
