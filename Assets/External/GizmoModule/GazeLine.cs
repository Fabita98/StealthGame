using System.Collections;
using System.Collections.Generic;
using Unity.Labs.SuperScience;
using UnityEngine;

public class GazeLine : MonoBehaviour
{

    public float cursorOffset, cursorRadius;

    public LayerMask mask;

    private Vector3 hitPosition;


    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(origin: transform.position, direction: transform.forward, out hit, Mathf.Infinity, layerMask: mask))
        {
            hitPosition = hit.point;
            cursorRadius = .5f;
        }      
    }

    private void Update()
    {
        GizmoModule.instance.DrawSphere(hitPosition + (transform.position - hitPosition).normalized * cursorOffset, cursorRadius, Color.red);
    }
}
