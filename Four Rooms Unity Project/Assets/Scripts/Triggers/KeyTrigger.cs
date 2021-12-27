using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTrigger : MonoBehaviour
{
    public Light light;
    public Light upperLight;
    public GameObject zombie;

    private void OnDestroy()
    {
        light.color = Color.red;
        light.intensity = 1.5f;
        
        upperLight.intensity = 1.5f;
        upperLight.color = Color.red;
        upperLight.GetComponent<Flikkering>().enabled = true;
        
        zombie.SetActive(true);
    }
}
