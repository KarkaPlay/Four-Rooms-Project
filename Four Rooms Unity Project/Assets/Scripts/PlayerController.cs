using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public TextMeshProUGUI tip;
    public Image crosshairImage;
    public Sprite[] crosshairs;
    public GameObject currentItem;
    public Light flashlight;
    public bool hasKey;
    
    private RaycastHit targetHit;
    private GameObject inventoryUI;
    private Inventory inventory;

    private void Awake()
    {
        inventoryUI = GameObject.Find("Inventory");
        inventory = transform.parent.parent.GetComponent<Inventory>();
    }

    void Update()
    {
        var rayDirection = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, rayDirection, out targetHit, 2.5f))
            switch (targetHit.transform.tag)
            {
                case "Key":
                case "Pickable":
                    MakeTipActive("Press 'E' to pick up");
                    if (Input.GetKeyDown(KeyCode.E))
                        targetHit.transform.gameObject.GetComponent<Pickup>().PickUp();
                    break;
                case "Light":
                    MakeTipActive("Press 'E' to switch the light");
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Light light = targetHit.transform.GetChild(0).gameObject.GetComponent<Light>();
                        light.enabled = !light.enabled;
                    }
                    break;
                case "Switch":
                    MakeTipActive("Press 'E' to switch the light");
                    if (Input.GetKeyDown(KeyCode.E))
                        targetHit.transform.GetComponent<Switch>().SwitchLight();
                    break;
                case "Door":
                    MakeTipActive("Press 'E' to open/close the door");
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Door doorControl = targetHit.transform.GetComponent<Door>();
                        hasKey = false;
                        int i = 0;
                        foreach (var slot in inventory.slots)
                        {
                            var item = slot.transform.GetChild(i);
                            if (item.CompareTag("Key"))
                            {
                                hasKey = true;
                                break;
                            }
                            i++;
                        }
                        if (doorControl.needKey)
                        {
                            if (hasKey)
                            {
                                doorControl.needKey = false;
                                doorControl.isOpen = !doorControl.isOpen;
                                //Destroy(inventory.items[0]);
                                //Destroy(inventoryUI.transform.GetChild(0));
                                inventory.RemoveSlot(i);
                            }
                        }
                        else
                        {
                            doorControl.isOpen = !doorControl.isOpen;
                        }
                    }
                    break;
                case "Locker":
                    MakeTipActive("Press 'E' to open/close the locker");
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Locker locker = targetHit.transform.GetComponent<Locker>();
                        locker.isOpen = !locker.isOpen;
                    }
                    break;
                default:
                    tip.gameObject.SetActive(false);
                    crosshairImage.sprite = crosshairs[0];
                    break;
            }
        else
        {
            tip.gameObject.SetActive(false);
            crosshairImage.sprite = crosshairs[0];
        }
        Debug.DrawRay(transform.position, rayDirection * 2.5f, Color.cyan);
        
        //if (!Keyboard.current.anyKey.wasPressedThisFrame) return;
        //if (!Input.anyKeyDown) return;

        for(int i=0; i<9; i++)
        {
            if(Input.GetKeyDown((KeyCode)(49+i)))
            {
                inventoryUI.transform.GetChild(i).GetComponent<Slot>().Grab();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }

    void MakeTipActive(string text)
    {
        tip.text = text;
        tip.gameObject.SetActive(true);
        crosshairImage.sprite = crosshairs[1];
    }
}
