using System.Collections.Generic;
using Unity.Labs.SuperScience;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class GazeLine : MonoBehaviour
    {
        [SerializeField] private float cursorOffset = .3f;
        [SerializeField] private float cursorRadius = .1f;

        public LayerMask mask;
        private int monkLayer, squareLayer;

        private Vector3 hitPosition;
        public static EyeInteractable staredMonk;

        private readonly List<EyeInteractable> eyeInteractables = new();

        private void Awake()
        {
            monkLayer = LayerMask.NameToLayer("Monks");
            squareLayer = LayerMask.NameToLayer("Squares");
        }

        void FixedUpdate() {
            EyeGazeRayCasting();
        }

        void Update() {
            GizmoModule.instance.DrawSphere(hitPosition + (transform.position - hitPosition).normalized * cursorOffset, cursorRadius, Color.cyan);
        }

        private void EyeGazeRayCasting() {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask: mask)) {
                
                hitPosition = hit.point;
                if (hit.collider.TryGetComponent<EyeInteractable>(out var eyeInteractable)) {
                    eyeInteractables.Add(eyeInteractable);
                    eyeInteractable.IsHovered = true;
                    if (eyeInteractable.gameObjLayer == monkLayer)
                    {
                        staredMonk = eyeInteractable;
                    }
                    EyeInteractable.HoveringTime += Time.fixedDeltaTime;
                }
                else UnSelect();
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
}