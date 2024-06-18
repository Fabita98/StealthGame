using Assets.Scripts.GazeTrackingFeature;
using System.Collections.Generic;
using Unity.Labs.SuperScience;
using UnityEngine;

public class GazeLine : MonoBehaviour
{
    public float cursorOffset, cursorRadius;

    public LayerMask mask;

    private Vector3 hitPosition;
    private List<EyeInteractable> eyeInteractables = new();

    void FixedUpdate()
    {
        if (Physics.Raycast(origin: transform.position, direction: transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask: mask)) {
            hitPosition = hit.point;
            cursorRadius = .5f;
            UnSelect();

            if (hit.collider.TryGetComponent<EyeInteractable>(out var eyeInteractable))
            {
                eyeInteractables.Add(eyeInteractable);
                eyeInteractable.IsHovered = true;
            }
        }
        else
        {
            UnSelect(true);
        }
    }

    private void Update()
    {
        GizmoModule.instance.DrawSphere(hitPosition + (transform.position - hitPosition).normalized * cursorOffset, cursorRadius, Color.red);
    }

    void UnSelect(bool clear = false)
    {
        foreach (EyeInteractable interactable in eyeInteractables)
        {
            interactable.IsHovered = false;
        }
        if (clear) eyeInteractables.Clear();
    }
}