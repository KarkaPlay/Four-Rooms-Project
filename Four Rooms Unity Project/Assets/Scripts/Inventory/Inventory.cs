using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public List<GameObject> slots = new List<GameObject>();
    public GameObject[] items;

    public GameObject slotPrefab;
    public GameObject inventory;

    public void AddSlot()
    {
        Debug.Log("добавляем");
        GameObject newSlot = Instantiate(slotPrefab, inventory.transform);
        slots.Add(newSlot);
        Debug.Log("добавили");
    }
}
