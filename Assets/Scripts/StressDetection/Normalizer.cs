using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Normalizer
{
    private float minValue;
    private float maxValue;

    public Normalizer(float minValue, float maxValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    public float Normalize(float value)
    {
        return (value - minValue) / (maxValue - minValue);
    }

    public float[] NormalizeArray(float[] values)
    {
        float[] normalizedValues = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            normalizedValues[i] = Normalize(values[i]);
        }
        return normalizedValues;
    }
}
