using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightHR : MonoBehaviour
{
    // Start is called before the first frame update
    int valueHR, range, averageHR;
    float newIntensity, lastDecrease=0,delta =0;
    bool reset = false;
    public bool abilityActive = false;
    public float maxEnergy = 30f; // Massimo tempo di energia disponibile per l'abilità
    public float energyDrainRate = 1f; // Energia consumata al secondo
    public float currentEnergy=15;
    void Start()
    {
        averageHR = 65;
    }
    private float incrementSpeed= 0.0005f;

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
        if (delta < -5)
        {
            reset = true;
        }

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ToggleAbility();
        }
        if (abilityActive)
        {
            UpdateAbility();
        }

    }
    void ToggleAbility()
        {
            if (abilityActive)
            {
                DeactivateAbility();
            }
            else if (currentEnergy > 0)
            {
                ActivateAbility();
            }
        }

    void ActivateAbility()
    {
        abilityActive = true;
        GetComponent<Light>().range = 76; 
        Debug.Log("Abilità attivata");
    }

    void DeactivateAbility()
    {
        GetComponent<Light>().range = 0;
        abilityActive = false;
        Debug.Log("Abilità disattivata");
    }

    void UpdateAbility()
    {
        if (currentEnergy > 0)
        {
            currentEnergy -= energyDrainRate * Time.deltaTime;
            if (currentEnergy <= 0)
            {
                currentEnergy = 0;
                DeactivateAbility();
            }
            else if (currentEnergy <= 2.2f)
            {
                OscillateLight();
            }
        }

        // Debug per mostrare l'energia corrente
        Debug.Log("Energia corrente: " + currentEnergy);
    }
    void OscillateLight()
    {
        float frequency = 2f; // Frequenza dell'oscillazione
        float intensity = Mathf.PingPong(Time.time * frequency, newIntensity);
        GetComponent<Light>().intensity = intensity;
    }
    // Funzione per ricaricare l'energia, se necessario
    public void RechargeEnergy(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
        Debug.Log("Energia ricaricata: " + currentEnergy);
    }
}
