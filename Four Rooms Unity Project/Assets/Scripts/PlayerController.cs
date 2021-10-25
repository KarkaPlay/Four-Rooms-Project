using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public TextMeshProUGUI tip;
    public Image crosshairImage;
    public Sprite[] crosshairs;
    public GameObject currentItem;
    
    private RaycastHit targetHit;
    private Ray ray;
    private GameObject _inventory;

    Camera camera;
    
    private void Awake()
    {
        camera = GetComponent<Camera>();
        _inventory = GameObject.Find("Inventory");
    }

    // Start is called before the first frame update
    void Start()
    {
        ray = camera.ScreenPointToRay(Input.mousePosition);
    }
    
    void Update()
    {
        var rayDirection = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, rayDirection, out targetHit, 2.5f))
        {
            if (targetHit.transform.CompareTag("Key") || targetHit.transform.CompareTag("Pickable"))
            {
                tip.text = "Press 'E' to pick up";
                tip.gameObject.SetActive(true);
                crosshairImage.sprite = crosshairs[1];
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("Ты нажал е пока смотрел на ключ");
                    targetHit.transform.gameObject.GetComponent<Pickup>().PickUp();
                }
            }
            else
            {
                tip.gameObject.SetActive(false);
                crosshairImage.sprite = crosshairs[0];
            }
        }
        else
        {
            tip.gameObject.SetActive(false);
        }
        Debug.DrawRay(transform.position, rayDirection * 2.5f, Color.cyan);

        try
        {

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        /*for(int i=0;i<9;i++)
        {
            if(Input.GetKeyDown((KeyCode)(48+i)))
            {
                _inventory.transform.GetChild(0).GetComponent<Slot>().Grab();
            }
        }*/

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _inventory.transform.GetChild(0).GetComponent<Slot>().Grab();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _inventory.transform.GetChild(1).GetComponent<Slot>().Grab();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _inventory.transform.GetChild(2).GetComponent<Slot>().Grab();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _inventory.transform.GetChild(3).GetComponent<Slot>().Grab();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _inventory.transform.GetChild(4).GetComponent<Slot>().Grab();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            _inventory.transform.GetChild(5).GetComponent<Slot>().Grab();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            _inventory.transform.GetChild(6).GetComponent<Slot>().Grab();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            _inventory.transform.GetChild(7).GetComponent<Slot>().Grab();
        }
    }
}
