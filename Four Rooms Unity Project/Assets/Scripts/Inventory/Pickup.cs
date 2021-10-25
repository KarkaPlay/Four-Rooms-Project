using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private Inventory _inventory;
    public GameObject slotSprite;
    private int maxLength = 8;

    private void Awake()
    {
        _inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
    }

    private void Start()
    {
        
    }

    public void PickUp()
    {
        Debug.Log("ключ начал подбираться");
        if (_inventory.slots.Count < maxLength)
        {
            Debug.Log("о, есть место");
            _inventory.AddSlot();
            AddToInventory();
        }
        else
        {
            Debug.Log("не, чел, места нет");
        }
    }

    void AddToInventory()
    {
        int i = _inventory.slots.Count - 1;
        var item = Instantiate(slotSprite, _inventory.slots[i].transform);

        Destroy(gameObject);
    }
}
