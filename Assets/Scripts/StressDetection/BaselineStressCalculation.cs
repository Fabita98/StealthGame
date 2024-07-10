using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaselineStressCalculation : MonoBehaviour
{
    public int baselineDuration = 5;
    private List<float> baselineData = new List<float>();
    private float baselineHRV;
    private bool isCollectingBaseline = true;

    void Update()
    {
        StartCoroutine(CollectBaseline());
    }

    IEnumerator CollectBaseline()
    {
        for (int i = 0; i < baselineDuration; i++)
        {
            float hrvValue = GetHRVData();
            baselineData.Add(hrvValue);
            yield return new WaitForSeconds(1);
        }

        baselineHRV = CalculateAverage(baselineData);
        isCollectingBaseline = false;
        // Debug.Log("Baseline HRV: " + baselineHRV);
    }

    float GetHRVData()
    {
        return Random.Range(50.0f, 70.0f);
    }

    float CalculateAverage(List<float> data)
    {
        float sum = 0.0f;
        foreach (float value in data)
        {
            sum += value;
        }
        return sum / data.Count;
    }

    public float GetBaselineHRV()
    {
        return baselineHRV;
    }

    public bool IsCollectingBaseline()
    {
        return isCollectingBaseline;
    }
}