using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public bool[] isFull;
    //public GameObject[] slots;
    public List<GameObject> slots = new List<GameObject>();
    public GameObject slot;
    public GameObject inventory;

    public void AddSlot()
    {
        Debug.Log("добавляем");
        GameObject newSlot = Instantiate(slot, inventory.transform);
        slots.Add(newSlot);
        Debug.Log("добавили");
    }
}
