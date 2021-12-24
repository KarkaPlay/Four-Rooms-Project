using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Flikkering : MonoBehaviour
{
    public bool isFlikkering = false;
    public float timeDelay;
    public float minDelay, maxDelay;

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
        timeDelay = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(timeDelay);
        
        gameObject.GetComponent<Light>().enabled = true;
        //gameObject.GetComponent<Light>().intensity *= 2;
        timeDelay = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(timeDelay);

        isFlikkering = false;
    }
}
