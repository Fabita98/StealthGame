using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.GazeTrackingFeature {
    [RequireComponent(typeof(EyeOutline))]
    [RequireComponent(typeof(AudioSource), typeof(AudioSource))] 
    internal class EyeInteractable : MonoBehaviour
    {
        #region Variables and events definition
        [Header("Eye hovering parameters")]
        private MeshRenderer meshRenderer;
        [SerializeField] private Material OnHoverActiveMaterial;
        [SerializeField] private Material OnHoverInactiveMaterial;
        // Event to be invoked to debug logger, if needed
        [SerializeField] private UnityEvent<GameObject> OnObjectHover;
        private EyeOutline eyeOutline;
        private readonly AudioSource[] audioSources = new AudioSource[2];
        [SerializeField] internal AudioSource snoringAudio, playerSpottedAudio;
        public bool IsHovered { get; set; }
        // isStaring is used to check if the player is staring at a monk;
        // while readyToTalk is used to check if the required amount of time to make the monk talk has passed
        public bool isStaring;
        public bool readyToTalk;
        public static float HoveringTime;
        [SerializeField] private float staringTime = 1.0f;
        [SerializeField] private float staringTimeToTalk = 2.0f;
        [SerializeField] private int minWidthValue = 0;
        [SerializeField] private int maxWidthValue = 4;
        private float duration;

        [Header("Voice Control")]
        private readonly OVRInput.RawButton keyForVoiceControl = OVRInput.RawButton.A;
        public static float buttonHoldTime;

        [Header("Game object layer")]
        public LayerMask gameObjLayer;  
        private int squareLayer, monkLayer;

        public static int OverallEyeInteractableInstanceCounter { get; private set; }
        public static event CounterChangeHandler OnCounterChanged;
        public delegate void CounterChangeHandler(int newCount);
        #endregion

        void Awake() => ComponentInit();      

        private void Update() => GazeControl();

        private void ComponentInit() {
            gameObjLayer = gameObject.layer;
            monkLayer = LayerMask.NameToLayer("Monks");
            squareLayer = LayerMask.NameToLayer("Squares");
            duration = staringTime + staringTimeToTalk;
            
            if (gameObjLayer == squareLayer) {
                if (TryGetComponent<MeshRenderer>(out var mR)) meshRenderer = mR;
                else Debug.LogWarning("MeshRenderer component not found.");
            } 

            if (gameObjLayer == monkLayer && TryGetComponent<EyeOutline>(out var eO)) {
                eyeOutline = eO;
                eyeOutline.enabled = false;

                // Ensure there are two AudioSource components
                var audioSourceComponents = GetComponents<AudioSource>();
                if (audioSourceComponents.Length < 2) {
                    for (int i = audioSourceComponents.Length; i < 2; i++) {
                        gameObject.AddComponent<AudioSource>();
                    }
                    audioSourceComponents = GetComponents<AudioSource>();
                }
                // Assign the AudioSource components to the array
                if (audioSourceComponents.Length >= 2) {
                    audioSources[0] = snoringAudio = audioSourceComponents[0];
                    audioSources[1] = playerSpottedAudio = audioSourceComponents[1];
                }
                else {
                    Debug.LogWarning("AudioSource array does not have enough elements.");
                }
            }
            else {
                Debug.LogWarning("EyeOutline component not found.");
            }
        }

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
            if (IsHovered) {
                OnObjectHover?.Invoke(gameObject);
                // Hovering square case 
                if (gameObjLayer == squareLayer && meshRenderer) meshRenderer.material = OnHoverActiveMaterial;

                // Hover for ONE monk at a time
                else if (gameObjLayer == monkLayer && HoveringTime > staringTime) {
                    isStaring = true;
                    OutlineWidthControl(isStaring, Color.yellow);
                    VocalKeyHoldingCheck();
                    // Hover to tell the player that he can speak to the hovered monk
                    if (HoveringTime > staringTimeToTalk && VocalKeyHoldingCheck()) {
                        isStaring = false;
                        readyToTalk = true;
                        OutlineWidthControl(readyToTalk, Color.green);
                        // Triggers the voice recording event 
                        EyeTrackingDebug.Instance.TriggerVoiceRecordingEvent();
                        readyToTalk = false;
                    }
                }
            } else ResetHover();
        }

        internal void ResetHover() {
            if (gameObjLayer == squareLayer && meshRenderer) meshRenderer.material = OnHoverInactiveMaterial;
            else if (gameObjLayer == monkLayer) {
                eyeOutline.enabled = false;
                isStaring = false;
            }
        }

        private bool VocalKeyHoldingCheck() {
            bool isButtonHeld = false;
            if (OVRInput.Get(keyForVoiceControl) && GazeLine.staredMonk.IsHovered) {
                StartCoroutine(GradualControllerVibration());
                buttonHoldTime += Time.deltaTime;
                if (buttonHoldTime > staringTime + staringTimeToTalk) {
                    isButtonHeld = true;
                }
            } else buttonHoldTime = 0f;
            return isButtonHeld;
        }

        internal void OutlineWidthControl(bool active, Color color) {
            if (active.Equals(false)) return;
            else {
                float lerpedWidth = Mathf.Lerp(minWidthValue, maxWidthValue, buttonHoldTime);
                eyeOutline.enabled = active;
                eyeOutline.OutlineColor = color;
                eyeOutline.OutlineMode = EyeOutline.Mode.OutlineAll;
                // isStaring is true until both staring AND key holding time checks are met
                eyeOutline.OutlineWidth = isStaring ? lerpedWidth : maxWidthValue;
            }
        }

        #region Vibration 
        internal IEnumerator GradualControllerVibration() { 
            float maxAmplitude = 1.0f;
            float maxFrequency = 1.0f;

            while (buttonHoldTime < duration) {
                float amplitude = Mathf.Lerp(0, maxAmplitude, buttonHoldTime / duration);
                float frequency = Mathf.Lerp(0, maxFrequency, buttonHoldTime / duration);
                OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
                yield return null;
            }

            // Ensure vibration is stopped at the end
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }

        internal void StartExplosiveVibration() {
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);
        }

        internal void StopExplosiveVibration() {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
        #endregion
        #endregion
    }
}