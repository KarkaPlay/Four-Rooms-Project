using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Sprite standartSlot;
    public Sprite chosenSlot;
    
    private Inventory _inventory;
    private PlayerController _playerController;

    private void Awake()
    {
        _inventory = GameObject.Find("Player").GetComponent<Inventory>();
        _playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    public void Grab()
    {
        foreach (var slot in _inventory.slots)
        {
            slot.GetComponent<Image>().sprite = standartSlot;
        }
        
        //Destroy(_playerController.currentItem);

        //Debug.Log("хочу взять в руку");
        gameObject.GetComponent<Image>().sprite = chosenSlot;
        switch (gameObject.transform.GetChild(0).gameObject.name)
        {
            case "Ключ (в инвентаре)(Clone)":
                _playerController.currentItem = Instantiate(_inventory.items[0], GameObject.Find("Player Camera").transform);
                break;
            case "Нож (в инвентаре)(Clone)":
                _playerController.currentItem = Instantiate(_inventory.items[1], GameObject.Find("Player Camera").transform);
                break;
        }
    }
}
