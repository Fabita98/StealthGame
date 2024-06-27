using Assets.Scripts.GazeTrackingFeature;
using System.Collections.Generic;
using Unity.Labs.SuperScience;
using UnityEngine;

public class GazeLine : MonoBehaviour
{
    public float cursorOffset, cursorRadius;

    public LayerMask mask;

    private Vector3 hitPosition;
    private Vector3 center1, center2;
    private float radius;

    private readonly List<EyeInteractable> eyeInteractables = new();

    void Start() {
        radius = 3f;
        center1 = transform.position;
        center2 = transform.position + Vector3.forward * radius;
    }

    void FixedUpdate() {
        EyeGazeRayCasting();
    }

    void Update() {
        // Eye gizmo
        GizmoModule.instance.DrawSphere(hitPosition + (transform.position - hitPosition).normalized * cursorOffset, cursorRadius, Color.red);    
    }

    private void EyeGazeRayCasting() {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask: mask)) {
            hitPosition = hit.point;
            cursorRadius = .5f;

            if (hit.collider.TryGetComponent<EyeInteractable>(out var eyeInteractable)) {
                eyeInteractables.Add(eyeInteractable);
                eyeInteractable.IsHovered = true;
                EyeInteractable.HoveringTime += Time.fixedDeltaTime;
            } else UnSelect();
        }
        else UnSelect(true);
    }

    void UnSelect(bool clear = false) {
        foreach (EyeInteractable interactable in eyeInteractables) {
            interactable.IsHovered = false;
            EyeInteractable.HoveringTime = 0;
        }
        if (clear) eyeInteractables.Clear();
    }
}