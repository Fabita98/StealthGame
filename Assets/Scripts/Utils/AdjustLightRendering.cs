using System;
using UnityEngine;

public class AdjustLightRendering : MonoBehaviour
{
    public Transform referencePoint;
    public float distanceThreshold = 10f;
    private Light[] lights;

    private void Start()
    {
        if (referencePoint == null)
        {
            referencePoint = MyPlayerController.Instance.transform;
        }
        lights = GetComponentsInChildren<Light>();
    }

    private void LateUpdate()
    {
        if (referencePoint == null)
        {
            Debug.LogWarning("Reference point is not set!");
            return;
        }

        foreach (Light light in lights)
        {
            float distance = Vector3.Distance(light.transform.position, referencePoint.position);
            if (distance <= distanceThreshold)
            {
                light.renderMode = LightRenderMode.ForcePixel; 
            }
            else
            {
                light.renderMode = LightRenderMode.Auto; 
            }
        }
    }
}