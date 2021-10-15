using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private Inventory _inventory;
    public GameObject slotButton;
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
        for (int i = 0; i < _inventory.slots.Count; i++)
        {
            if (!_inventory.isFull[i])
            {
                Debug.Log("о, есть место");
                Instantiate(slotButton, _inventory.slots[i].transform);

                Destroy(gameObject);
                _inventory.isFull[i] = true;
                break;
            }
            else
            {
                Debug.Log("не, чел, места нет");
                if (_inventory.slots.Count < maxLength)
                {
                    Debug.Log("ща добавим");
                    _inventory.AddSlot();
                }

                break;
            }
        }
    }
}
