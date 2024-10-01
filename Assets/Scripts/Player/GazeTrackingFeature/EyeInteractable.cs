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
        [Header("Squares related")]
        internal MeshRenderer meshRenderer;
        [SerializeField] internal Material OnHoverActiveMaterial;
        [SerializeField] internal Material OnHoverInactiveMaterial;
        // Event to be invoked to debug logger, if needed
        [SerializeField] private UnityEvent<GameObject> OnObjectHover;
        internal EyeOutline eyeOutline;
        internal readonly AudioSource[] audioSources = new AudioSource[2];
        [SerializeField] internal AudioSource snoringAudio, playerSpottedAudio;
        public bool IsHovered { get; set; }
        // isStaring is used to check if the player is staring at a monk after a minimum amount of time;
        // while readyToTalk is used to check if the required amount of time to make the monk snore has passed
        public bool isStaring;
        public bool readyToTalk;
        internal static float HoveringTime;
        [SerializeField] private float staringTimeToPressVocalKey = 1.0f;
        [SerializeField] private float staringTimeToTalk = 2.0f;
        private float duration;
        [SerializeField] private float minWidthValue = .2f;
        [SerializeField] private float maxWidthValue = 4;

        [Header("Voice Control")]
        private readonly OVRInput.RawButton keyForVoiceControl = OVRInput.RawButton.A;
        public static float buttonHoldTime;

        [Header("Game object layer")]
        public LayerMask gameObjLayer;  

        public static int OverallEyeInteractableInstanceCounter { get; private set; }
        public static event CounterChangeHandler OnCounterChanged;
        public delegate void CounterChangeHandler(int newCount);
        #endregion

        void Awake() => ComponentInit();      

        private void Update() => GazeControl();

        private void ComponentInit() {
            gameObjLayer = gameObject.layer;
            duration = staringTimeToPressVocalKey + staringTimeToTalk;
            
            if (gameObjLayer == GazeLine.Instance.squareLayer) {
                if (TryGetComponent<MeshRenderer>(out var mR)) meshRenderer = mR;
                else Debug.LogWarning("MeshRenderer component not found.");
            } 

            if (gameObjLayer == GazeLine.Instance.monkLayer && TryGetComponent<EyeOutline>(out var eO)) {
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
                // Case 1: Hovering monk -> blue outline
                if (GazeLine.staredMonk.IsHovered) {
                    OutlineWidthControl(IsHovered, Color.blue);

                    // Case 1.1 : Keep staring at the monk -> switch to yellow outline && Vocal Key enabled
                    if (HoveringTime > staringTimeToPressVocalKey) {
                        isStaring = true;
                        OutlineWidthControl(isStaring, Color.yellow);
                        VocalKeyHoldingCheck();
                        // Case 1.1.1 : Keep staring at the monk after having both stared at it
                        // and held the key for required amount of time -> switch to green outline && starts snoring
                        if (HoveringTime > staringTimeToTalk && VocalKeyHoldingCheck()) {
                            isStaring = false;
                            StartRightControllerVibrationCoroutine();
                            readyToTalk = true;
                            OutlineWidthControl(readyToTalk, Color.green);
                            // Triggers the voice recording event 
                            EyeTrackingDebug.Instance.TriggerVoiceRecordingEvent();
                            readyToTalk = false;
                        }
                    }
                }
            } else GazeLine.Instance.UnSelect();
        }

        private bool VocalKeyHoldingCheck() {
            bool isButtonHeld = false;
            if (OVRInput.Get(keyForVoiceControl) && GazeLine.staredMonk.IsHovered) {
                StartCoroutine(GradualControllerVibration());
                buttonHoldTime += Time.deltaTime;
                if (buttonHoldTime >= duration) {
                    isButtonHeld = true;
                }
            } else buttonHoldTime = 0f;
            return isButtonHeld;
        }

        internal void OutlineWidthControl(bool active, Color color) {
            if (active.Equals(false)) return;
            
            float lerpedWidth = Mathf.Lerp(minWidthValue, maxWidthValue, buttonHoldTime);
            eyeOutline.OutlineMode = EyeOutline.Mode.OutlineAll;
            eyeOutline.enabled = active;
            eyeOutline.OutlineColor = color;
            eyeOutline.OutlineWidth = active switch {
                true when GazeLine.staredMonk.IsHovered => maxWidthValue/2,
                true when (GazeLine.staredMonk.IsHovered && isStaring) => lerpedWidth,
                true when readyToTalk => maxWidthValue,
                _ => minWidthValue,
            };
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
            StopRightControllerVibration();        
        }

        internal void StartRightControllerVibrationCoroutine() {
            StartCoroutine(RightControllerVibrationCoroutine());
        }

        internal IEnumerator RightControllerVibrationCoroutine() {
            float vibDuration = 2.0f;
            byte maxAmplitude = 2;
            byte maxFrequency = 1;

            OVRInput.SetControllerVibration(maxFrequency, maxAmplitude, OVRInput.Controller.RTouch);

            // Wait for the duration
            yield return new WaitForSeconds(vibDuration);

            StopRightControllerVibration();
        }

        internal void StopRightControllerVibration() {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
        #endregion
        #endregion
    }
}