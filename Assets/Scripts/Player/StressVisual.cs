using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressVisual : MonoBehaviour
{
    [Range(0, 255)]
    public int colorValue = 0;
    
    private Renderer objectRenderer;
    
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        UpdateColor(colorValue);
    }
    
    void Update()
    {
        UpdateColor(colorValue);
    }
    
    void UpdateColor(float value)
    {
        float normalizedValue = colorValue / 255f;
        Color newColor = new Color(normalizedValue, normalizedValue, normalizedValue);
        objectRenderer.material.color = newColor;
    }
}
