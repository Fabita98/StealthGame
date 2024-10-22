using System.Collections.Generic;
using Unity.Labs.SuperScience;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class GazeLine : MonoBehaviour {
        [SerializeField] private float cursorOffset = .1f;
        [SerializeField] private float cursorRadius = .2f;

        internal static LayerMask mask;
        internal static int monkLayer, squareLayer;

        internal static Vector3 hitPosition;
        internal static EyeInteractable staredMonk = null;
        private static readonly List<EyeInteractable> eyeInteractablesList = new();

        private static int instanceCount = 0;
        private const int maxInstances = 2;

        private void Awake() {
            if (instanceCount > maxInstances) {
                Destroy(gameObject);
                return;
            }

            monkLayer = LayerMask.NameToLayer("Monks");
            squareLayer = LayerMask.NameToLayer("Squares");
            mask = LayerMask.GetMask("Monks", "Squares");
        }

        private void OnEnable() {
            instanceCount++;
        }

        private void OnDestroy() {
            instanceCount--;
        }

        void FixedUpdate() => EyeGazeRayCasting();

        void Update() => GizmoModule.instance.DrawSphere(hitPosition + (transform.position - hitPosition).normalized * cursorOffset, cursorRadius, Color.cyan);

        private void EyeGazeRayCasting() {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask: mask)) {
                hitPosition = hit.point;

                if (hit.collider.TryGetComponent<EyeInteractable>(out var eyeInteractable)) {
                    eyeInteractablesList.Add(eyeInteractable);
                    eyeInteractable.IsHovered = true;

                    // Case 0: Hovering square 
                    if (eyeInteractable.gameObject.layer == squareLayer && eyeInteractable.TryGetComponent<MeshRenderer>(out var mR)) {
                        mR.material = eyeInteractable.OnHoverActiveMaterial;
                    }
                    // Case 1: Hovering monk
                    else if (eyeInteractable.gameObject.layer == monkLayer) {
                        staredMonk = eyeInteractable;
                        EyeInteractable.HoveringTime += Time.fixedDeltaTime;
                    }
                }
                else UnSelect();
            }
            else UnSelect(true);
        }

        public void UnSelect(bool clear = false) {
            foreach (EyeInteractable interactable in eyeInteractablesList) {
                interactable.IsHovered = false;
                EyeInteractable.HoveringTime = 0;
                // Case 0: Unhovering square
                if (interactable.gameObject.layer == squareLayer && interactable.TryGetComponent<MeshRenderer>(out var mR))
                    mR.material = interactable.OnHoverInactiveMaterial;
                // Case 1: Unhovering monk
                else if (interactable.gameObject.layer == monkLayer) {
                    interactable.eyeOutline.OutlineWidth = EyeTrackingDebug.noWidthValue;
                    interactable.isBeingStared = false;
                }
            }
            if (clear) eyeInteractablesList.Clear();
        }
    }
}