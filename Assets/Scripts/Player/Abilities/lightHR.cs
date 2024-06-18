using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightHR : MonoBehaviour
{
    // Start is called before the first frame update
    int valueHR, range, averageHR;
    float newIntensity, lastDecrease=0,delta =0;
    bool reset = false;
    void Start()
    {
        averageHR = 65;
    }
    private float incrementSpeed= 0.0005f;

    // Metodo per avviare l'incremento del valore da start a finish in 5 secondi
    /*public void IncrementValue()
    {
        startValue = this.GetComponent<Light>().range;
        targetValue = newIntensity;
        incrementSpeed = (targetValue - startValue) / 4f; // Calcolo della velocità di incremento per completare in 5 secondi
        StartCoroutine(IncrementCoroutine());
    }

    // Coroutine per incrementare gradualmente il valore
    private IEnumerator IncrementCoroutine()
    {
        float timer = 0f;
        while (timer < 4f)
        {
            GetComponent<Light>().range += incrementSpeed * Time.deltaTime; // Incremento graduale del valore nel tempo
            timer += Time.deltaTime;
            yield return null;
        }
        currentValue = targetValue; // Assicurati che il valore finale sia esatto
    }*/

    // Update is called once per frame
    void Update()
    {
        valueHR = hyperateSocket.value;
        averageHR = HeartbeatManager.avgINT;
        newIntensity = 1 - (valueHR - (averageHR+delta))/10f;
        newIntensity = Mathf.Clamp(newIntensity, 0, 1);
        if (GetComponent<Light>().intensity < newIntensity)
        {
            GetComponent<Light>().intensity += incrementSpeed;
        }
        if (GetComponent<Light>().intensity > newIntensity)
        {
            GetComponent<Light>().intensity -= incrementSpeed;
            lastDecrease = Time.time;
        }
        if (Time.time - lastDecrease > 15&& !reset)
        {
            delta -= 1;
            lastDecrease = Time.time;
        }
        if(delta < -5)
        {
            reset = true;
        }
    }
}
