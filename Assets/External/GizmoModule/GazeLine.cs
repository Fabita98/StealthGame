using Assets.Scripts.GazeTrackingFeature;
using System.Collections.Generic;
using Unity.Labs.SuperScience;
using UnityEngine;

public class GazeLine : MonoBehaviour
{
    public float cursorOffset, cursorRadius;

    public LayerMask mask;

    private Vector3 hitPosition;
    private readonly List<EyeInteractable> eyeInteractables = new();

    void FixedUpdate() {
        EyeGazeRayCasting();
    }

    private void Update()
    {
        GizmoModule.instance.DrawSphere(hitPosition /*+ (transform.position - hitPosition).normalized * cursorOffset*/, cursorRadius, Color.red);
    }

    private void EyeGazeRayCasting()
    {
        if (Physics.Raycast(origin: transform.position, direction: transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask: mask))
        {
            UnSelect();
            hitPosition = hit.point;
            cursorRadius = .5f;

            if (hit.collider.TryGetComponent<EyeInteractable>(out var eyeInteractable))
            {
                eyeInteractables.Add(eyeInteractable);
                eyeInteractable.IsHovered = true;
                //eyeInteractable.HoveringTime += Time.fixedDeltaTime;
            }
        }
        else UnSelect(true);
    }

    void UnSelect(bool clear = false)
    {
        foreach (EyeInteractable interactable in eyeInteractables)
        {
            interactable.ResetHover();
        }
        if (clear) eyeInteractables.Clear();
    }
}