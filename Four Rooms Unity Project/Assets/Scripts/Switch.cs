using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public Light light;

    public void SwitchLight()
    {
        light.enabled = !light.enabled;
    }
}
