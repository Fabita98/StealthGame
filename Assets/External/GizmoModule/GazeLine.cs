using System.Collections.Generic;
using Unity.Labs.SuperScience;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class GazeLine : MonoBehaviour
    {
        [SerializeField] private float cursorOffset = .1f;
        [SerializeField] private float cursorRadius = .2f;

        public LayerMask mask;
        public int monkLayer, squareLayer;

        private Vector3 hitPosition;
        public static EyeInteractable staredMonk = null;
        public static GazeLine Instance { get; private set; }
        private readonly List<EyeInteractable> eyeInteractablesList = new();

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            }
            else Destroy(gameObject);

            monkLayer = LayerMask.NameToLayer("Monks");
            squareLayer = LayerMask.NameToLayer("Squares");
        }

        void FixedUpdate() => EyeGazeRayCasting();

        //void Update() => GizmoModule.instance.DrawSphere(hitPosition + (transform.position - hitPosition).normalized * cursorOffset, cursorRadius, Color.cyan);

        private void EyeGazeRayCasting() {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask: mask)) {
                
                hitPosition = hit.point;
                if (hit.collider.TryGetComponent<EyeInteractable>(out var eyeInteractable)) {
                    eyeInteractablesList.Add(eyeInteractable);
                    eyeInteractable.IsHovered = true;

                    // Case 0: Hovering square 
                    if (eyeInteractable.gameObjLayer == squareLayer && eyeInteractable.TryGetComponent<MeshRenderer>(out var mR)) {
                        mR.material = eyeInteractable.OnHoverActiveMaterial;
                    }
                    // Case 1: Hovering monk
                    else if (eyeInteractable.gameObjLayer == monkLayer) {
                        staredMonk = eyeInteractable;
                        EyeInteractable.HoveringTime += Time.fixedDeltaTime;
                    }
                } else UnSelect();
            } else UnSelect(true);
        }

        public void UnSelect(bool clear = false) {
            staredMonk = null;
            foreach (EyeInteractable interactable in eyeInteractablesList) {
                interactable.IsHovered = false;
                EyeInteractable.HoveringTime = 0;
                // Case 0: Unhovering square
                if (interactable.gameObjLayer == squareLayer && interactable.TryGetComponent<MeshRenderer>(out var mR)) 
                    mR.material = interactable.OnHoverInactiveMaterial;
                // Case 1: Unhovering monk
                else if (interactable.gameObjLayer == monkLayer) {
                        interactable.eyeOutline.enabled = false;
                        interactable.isStaring = false;
                }
            }
            if (clear) eyeInteractablesList.Clear();
        }
    } 
}