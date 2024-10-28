using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    [RequireComponent(typeof(EyeOutline))]
    [RequireComponent(typeof(AudioSource), typeof(AudioSource))]
    [RequireComponent(typeof(Collider), typeof(Collider))]
    internal class EyeInteractable : MonoBehaviour {
        #region Variables and events definition
        [Header("Eye hovering parameters")]
        internal static float HoveringTime;
        public bool IsHovered { get; set; }
        public bool isBeingStared;
        public bool readyToTalk;
        public bool isSleeping = false;
        internal Collider[] colliders;
        private Collider standardCollider;
        private Collider eyeTrackingCollider;
        [SerializeField] private const float eyeTrackingColliderRadius = 2.5f;
        [SerializeField] private const float eyeTrackingColliderHeight = 8.9f;

        [Header("Squares related")]
        internal MeshRenderer meshRenderer;
        [SerializeField] internal Material OnHoverActiveMaterial;
        [SerializeField] internal Material OnHoverInactiveMaterial;

        [Header("Snoring Audio Playback")]
        internal EyeOutline eyeOutline;
        internal AudioSource[] audioSources;
        public static float snoringCooldownEndTime = 15.0f;
        public static AudioClip snoringAudio;
        internal AudioClip playerSpottedAudio;

        public static int OverallEyeInteractableInstanceCounter { get; private set; }
        public static event CounterChangeHandler OnEyeInteractableInstancesCounterChanged;
        public delegate void CounterChangeHandler(int newCount);
        #endregion
        
        #region EyeInteractable instances counter
        void OnEnable() {
            OverallEyeInteractableInstanceCounter++;
            OnEyeInteractableInstancesCounterChanged?.Invoke(OverallEyeInteractableInstanceCounter);
        }

        void OnDisable() {
            OverallEyeInteractableInstanceCounter--;
            OnEyeInteractableInstancesCounterChanged?.Invoke(OverallEyeInteractableInstanceCounter);
        }
        #endregion

        void Awake() => ComponentInit();

        void Start() {
            if (gameObject.layer == GazeLine.monkLayer) 
                InitializeAudioSources();
        }        

        #region Initialization methods
        private void ComponentInit() {
            if (TryGetComponent<EyeOutline>(out var eO)) {
                eyeOutline = eO;
                eyeOutline.OutlineWidth = EyeTrackingDebug.noWidthValue;
            }
            else Debug.LogWarning("EyeOutline component not found on: " + name);

            if (gameObject.layer == GazeLine.squareLayer) {
                if (TryGetComponent<MeshRenderer>(out var mR)) meshRenderer = mR;
                else Debug.LogWarning("MeshRenderer component not found on square: " + name);
            }

            else if (gameObject.layer == GazeLine.monkLayer) {
                InitializeColliders();
            }
        }

        private void InitializeColliders() {
            colliders = GetComponents<Collider>();
            standardCollider = colliders[0];
            eyeTrackingCollider = colliders[1];

            if (colliders.Length < 2) {
                for (int i = colliders.Length; i < 2; i++) {
                    gameObject.AddComponent<CapsuleCollider>();
                }
                colliders = GetComponents<Collider>();
            }
            else if (colliders.Length >= 2) {
                standardCollider.isTrigger = false;
                eyeTrackingCollider.isTrigger = true;
                // Bigger collider for eye tracking because monks are currently moving very fast
                if (eyeTrackingCollider.TryGetComponent<CapsuleCollider>(out var cC)) {
                    cC.radius = eyeTrackingColliderRadius;
                    cC.height = eyeTrackingColliderHeight;
                }
            }
            else Debug.LogWarning("Collider array does not have enough elements.");
        }

        private void InitializeAudioSources() {
            audioSources = GetComponents<AudioSource>();

            // Ensure there are two AudioSource components
            if (audioSources.Length < 2) {
                for (int i = audioSources.Length; i < 2; i++) {
                    gameObject.AddComponent<AudioSource>();
                }
                audioSources = GetComponents<AudioSource>();
            }
            // Assign the AudioSource components to the array
            else if (audioSources.Length >= 2) {
                audioSources[0].clip = snoringAudio;
                if (AudioHolder.instance != null) {
                    playerSpottedAudio = AudioHolder.instance.AssignRandomPlayerSpottedAudio();
                    audioSources[1].clip = playerSpottedAudio;
                    Debug.Log("playerSpottedAudio assigned ");
                }
            }
            else Debug.LogWarning("AudioSource array does not have enough elements.");

            // Ensure snoringAudio is assigned
            if (snoringAudio == null) {
                Debug.LogError("snoringAudio is not assigned.");
            }
        }
        #endregion

        #region Player spotted audio playback
        private IEnumerator PlayPlayerSpottedAudioCoroutine() {
            if (audioSources[1].isPlaying) yield break;

            if (playerSpottedAudio != null) {
                audioSources[1].Play();
                yield return new WaitForSeconds(playerSpottedAudio.length);
                audioSources[1].Stop();
            }
            else {
                Debug.LogWarning("AudioSource[1] is null.");
                yield break;
            }
        }

        public void StartPlayerSpottedAudioCoroutine() => StartCoroutine(PlayPlayerSpottedAudioCoroutine());
        #endregion
    }
}