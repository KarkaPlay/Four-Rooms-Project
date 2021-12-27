using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorClosingTrigger : MonoBehaviour
{
    public GameObject door;
    public GameObject entrance;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        door.GetComponent<Door>().isOpen = !door.GetComponent<Door>().isOpen;
        Destroy(door.GetComponent<Collider>());
        StartCoroutine(RemoveEntrance());
        GameObject.Find("Player Camera").transform.GetChild(0).GetComponent<Light>().intensity = 2;
        GameObject.Find("Player Camera").transform.GetChild(0).GetComponent<Light>().range = 25;
    }

    IEnumerator RemoveEntrance()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(entrance);
    }
}
