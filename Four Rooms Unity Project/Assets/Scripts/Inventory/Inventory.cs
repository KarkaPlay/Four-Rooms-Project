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
    public GameObject inventoryUI;

    public void AddSlot()
    {
        //Debug.Log("добавляем");
        GameObject newSlot = Instantiate(slotPrefab, inventoryUI.transform);
        slots.Add(newSlot);
        //Debug.Log("добавили");
    }

    public void RemoveSlot(int i)
    {
        //Destroy(slots[i].transform.GetChild(0).GetComponent<Image>());
        //Destroy(slots[i].GetComponent<Image>());
        slots.Remove(slots[i]);
        DestroyImmediate(inventoryUI.transform.GetChild(i).gameObject, true);
        DestroyImmediate(items[i], true);
    }
}
