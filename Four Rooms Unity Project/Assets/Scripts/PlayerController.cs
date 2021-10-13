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
    
    private RaycastHit targetHit;
    private Ray ray;

    Camera camera;
    
    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ray = camera.ScreenPointToRay(Input.mousePosition);
    }
    
    void FixedUpdate()
    {
        var rayDirection = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, rayDirection, out targetHit, 2.5f))
        {
            if (targetHit.transform.CompareTag("Key"))
            {
                tip.text = "Press 'E' to pick up the key";
                tip.gameObject.SetActive(true);
                crosshairImage.sprite = crosshairs[1];
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
    }
}
