using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.GazeTrackingFeature
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    internal class EyeInteractable : MonoBehaviour
    {
        public bool IsHovered { get; set; }
        [SerializeField] private UnityEvent<GameObject> OnObjectHover;
        [SerializeField] private Material OnHoverActiveMaterial; 
        [SerializeField] private Material OnHoverInactiveMaterial; 
        private MeshRenderer meshRenderer;

        void Start() => meshRenderer = GetComponent<MeshRenderer>();

        private void Update()
        {
            if (IsHovered) { 
                meshRenderer.material = OnHoverActiveMaterial;
                OnObjectHover?.Invoke(gameObject);
            }
            else
            {
                meshRenderer.material = OnHoverInactiveMaterial;
            }
        }
    }
}