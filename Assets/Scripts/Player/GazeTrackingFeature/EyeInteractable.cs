using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.GazeTrackingFeature {
    [RequireComponent(typeof(EyeOutline))]
    [RequireComponent(typeof(AudioSource), typeof(AudioSource))]
    [RequireComponent(typeof(Collider), typeof(Collider))]
    internal class EyeInteractable : MonoBehaviour {
        #region Variables and events definition
        [Header("Eye hovering parameters")]
        internal Collider[] colliders;
        private Collider standardCollider;
        private Collider eyeTrackingCollider;
        public bool IsHovered { get; set; }
        // isStaring is used to check if the player is staring at a monk after a minimum amount of time;
        // while readyToTalk is used to check if the required amount of time to make the monk snore has passed
        public bool isBeingStared;
        public bool readyToTalk;
        internal static float HoveringTime;
        [SerializeField] private float staringTimeToPressVocalKey = 2.0f;
        [SerializeField] private float staringTimeToTalk = 4.0f;
        private float duration;
        internal const byte noWidthValue = 0;
        [SerializeField] private float minWidthValue = .2f;
        [SerializeField] private float maxWidthValue = 4;

        [Header("Squares related")]
        internal MeshRenderer meshRenderer;
        [SerializeField] internal Material OnHoverActiveMaterial;
        [SerializeField] internal Material OnHoverInactiveMaterial;
        // Event to be invoked to debug logger, if needed
        [SerializeField] private UnityEvent<GameObject> OnObjectHover;

        [Header("Audio Playback")]
        private readonly OVRInput.RawButton keyForVoiceControl = OVRInput.RawButton.B;
        public static float buttonHoldTime;
        internal EyeOutline eyeOutline;
        internal AudioSource[] audioSources;
        public static AudioClip snoringAudio;
        internal AudioClip playerSpottedAudio;

        [Header("Vibration")]
        private bool isVibrating;

        public static int OverallEyeInteractableInstanceCounter { get; private set; }
        public static event CounterChangeHandler OnCounterChanged;
        public delegate void CounterChangeHandler(int newCount);
        #endregion

        void Awake() => ComponentInit();

        void Start() {
            if (gameObject.layer == GazeLine.monkLayer) 
                InitializeAudioSources();
        }

        private void Update() => GazeControl();

        #region Initialization methods
        private void ComponentInit() {
            duration = staringTimeToPressVocalKey + staringTimeToTalk;

            if (TryGetComponent<EyeOutline>(out var eO)) {
                eyeOutline = eO;
                eyeOutline.OutlineWidth = noWidthValue;
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
                if (AudioHolder.instance.AssignRandomPlayerSpottedAudio() != null) {
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

        #region EyeInteractable instances counter
        void OnEnable() {
            OverallEyeInteractableInstanceCounter++;
            OnCounterChanged?.Invoke(OverallEyeInteractableInstanceCounter);
        }

        void OnDisable() {
            OverallEyeInteractableInstanceCounter--;
            OnCounterChanged?.Invoke(OverallEyeInteractableInstanceCounter);
        }
        #endregion

        #region Eye control 
        public void GazeControl() {
            if (GazeLine.staredMonk != null && GazeLine.staredMonk.IsHovered) {
                OnObjectHover?.Invoke(gameObject);
                // Case 1: Hovering monk -> blue outline
                OutlineWidthControl(Color.blue);

                // Case 1.1 : Keep staring at the monk -> switch to yellow outline && Vocal Key enabled
                if (HoveringTime > staringTimeToPressVocalKey) {
                    isBeingStared = true;
                    OutlineWidthControl(Color.yellow);
                    // Case 1.1.1 : Keep staring at the monk after having both stared at it
                    // and held the key for required amount of time -> switch to green outline && starts snoring
                    if (HoveringTime > staringTimeToTalk && VocalKeyHoldingCheck()) {
                        isBeingStared = false;
                        readyToTalk = true;
                        StartRightControllerStrongVibrationCoroutine();
                        OutlineWidthControl(Color.green);
                        if (snoringAudio != null) EyeTrackingDebug.Instance.SnoringAudioPlaybackTrigger();
                        else Debug.LogWarning("snoringAudio is null -> Event not triggered!");
                        readyToTalk = false;
                    }
                }
            }
            else return;
        }

        private bool VocalKeyHoldingCheck() {
            bool isButtonHeld = false;
            if (OVRInput.Get(keyForVoiceControl) && GazeLine.staredMonk.isBeingStared) {
                StartCoroutine(GradualControllerVibration());
                buttonHoldTime += Time.deltaTime;
                if (buttonHoldTime >= duration) {
                    isButtonHeld = true;
                }
            }
            else buttonHoldTime = 0f;
            return isButtonHeld;
        }

        internal void OutlineWidthControl(Color color) {
            bool active = GazeLine.staredMonk.IsHovered;
            EyeInteractable staredMonk = GazeLine.staredMonk;

            if (active.Equals(false)) return;
            else if (!staredMonk.eyeOutline) {
                Debug.LogWarning("EyeOutline component is not assigned.");
                return;
            }

            float lerpedWidth = Mathf.Lerp(minWidthValue, maxWidthValue, buttonHoldTime);
            float desiredWidth = active switch {
                true when staredMonk.IsHovered => staredMonk.maxWidthValue / 2,
                true when staredMonk.IsHovered && staredMonk.isBeingStared => lerpedWidth,
                true when staredMonk.IsHovered && staredMonk.readyToTalk => maxWidthValue,
                _ => noWidthValue,
            };

            staredMonk.eyeOutline.OutlineMode = EyeOutline.Mode.OutlineAll;
            staredMonk.eyeOutline.OutlineColor = color;
            staredMonk.eyeOutline.OutlineWidth = desiredWidth;
        }

        #region Vibration 
        internal IEnumerator GradualControllerVibration() {
            if (isVibrating) yield break;
            isVibrating = true;

            float maxAmplitude = 1.0f;
            float maxFrequency = 1.0f;

            while (buttonHoldTime < duration) {
                float amplitude = Mathf.Lerp(0, maxAmplitude, buttonHoldTime / duration);
                float frequency = Mathf.Lerp(0, maxFrequency, buttonHoldTime / duration);
                OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
                yield return null;
            }

            // Ensure vibration is stopped at the end
            StopRightControllerVibration();
        }

        internal void StartRightControllerStrongVibrationCoroutine() {
            if (isVibrating) return;
            StartCoroutine(RightControllerStrongVibrationCoroutine());
        }

        internal IEnumerator RightControllerStrongVibrationCoroutine() {
            if (isVibrating) yield break;
            isVibrating = true;

            float vibDuration = 2.0f;
            byte maxAmplitude = 1;
            byte maxFrequency = 1;

            OVRInput.SetControllerVibration(maxFrequency, maxAmplitude, OVRInput.Controller.RTouch);

            // Wait for the duration
            yield return new WaitForSeconds(vibDuration);

            StopRightControllerVibration();
        }

        internal void StopRightControllerVibration() {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            isVibrating = false;
        }
        #endregion
        #endregion

        #region Player spotted audio playback

        private IEnumerator PlayPlayerSpottedAudioCoroutine() {
            if (playerSpottedAudio != null) {
                audioSources[1].Play();
                yield return new WaitForSeconds(playerSpottedAudio.length);
                audioSources[1].Stop();
            }
            else {
                Debug.LogWarning("AudioSource[1] is null on staredMonkForSingleton.");
                yield break;
            }
        }

        public void StartPlayerSpottedAudioCoroutine() => StartCoroutine(PlayPlayerSpottedAudioCoroutine());

        private void HandlePlayerSpottedAudioPlayback() {
            StartPlayerSpottedAudioCoroutine();
        }
        #endregion
    }
}