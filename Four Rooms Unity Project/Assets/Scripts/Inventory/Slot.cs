using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Sprite standartSlot;
    public Sprite chosenSlot;
    
    public Inventory _inventory;
    public PlayerController _playerController;

    private void Start()
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
        Destroy(_playerController.currentItem);
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
