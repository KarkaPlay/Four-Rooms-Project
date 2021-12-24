using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Flikkering : MonoBehaviour
{
    public bool isFlikkering = false;
    public float timeDelay;
    
    private void Update()
    {
        if (!isFlikkering)
        {
            StartCoroutine(FlikkeringLight());
        }
    }

    IEnumerator FlikkeringLight()
    {
        isFlikkering = true;

        gameObject.GetComponent<Light>().enabled = false;
        //.GetComponent<Light>().intensity /= 2;
        timeDelay = Random.Range(0.01f, 0.2f);
        yield return new WaitForSeconds(timeDelay);
        
        gameObject.GetComponent<Light>().enabled = true;
        //gameObject.GetComponent<Light>().intensity *= 2;
        timeDelay = Random.Range(0.01f, 0.2f);
        yield return new WaitForSeconds(timeDelay);

        isFlikkering = false;
    }
}
