using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature
{
    [RequireComponent(typeof(LineRenderer))]
    public class TrackingRay : MonoBehaviour
    {
        [SerializeField] private float rayDistance = 10.0f;
        [SerializeField] private float rayWidth = 0.01f;
        [SerializeField] private LayerMask layersToInclude;
        [SerializeField] private Color rayColorDefaultState = Color.yellow;
        [SerializeField] private Color rayColorHoverState = Color.red;

        private LineRenderer lineRenderer;
        private List<EyeInteractable> eyeInteractables = new();

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            SetupRay();
        }

        void FixedUpdate()
        {
            Vector3 rayCastDirection = transform.TransformDirection(Vector3.forward) * rayDistance;

            if (Physics.Raycast(transform.position, rayCastDirection, out RaycastHit hit, Mathf.Infinity, layersToInclude))
            {
                UnSelect();
                lineRenderer.startColor = rayColorHoverState;
                lineRenderer.endColor = rayColorHoverState;
                if (hit.collider.TryGetComponent<EyeInteractable>(out var eyeInteractable))
                {
                    eyeInteractables.Add(eyeInteractable);
                    eyeInteractable.IsHovered = true;
                }
            }
            else
            {
                lineRenderer.startColor = rayColorDefaultState;
                lineRenderer.endColor = rayColorDefaultState;
                UnSelect(true);
            }
        }

        #region Public Methods
        void SetupRay()
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = rayWidth;
            lineRenderer.endWidth = rayWidth;
            lineRenderer.startColor = rayColorDefaultState;
            lineRenderer.endColor = rayColorDefaultState;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y,
                                                    transform.position.z + rayDistance));
        }

        void UnSelect(bool clear = false)
        {
            foreach (EyeInteractable interactable in eyeInteractables)
            {
                interactable.IsHovered = false;
            }
            if (clear) eyeInteractables.Clear();
        }
        #endregion
    }
}