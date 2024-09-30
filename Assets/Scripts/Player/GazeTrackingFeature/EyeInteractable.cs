using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.GazeTrackingFeature {
    [RequireComponent(typeof(EyeOutline))]
    [RequireComponent(typeof(AudioSource))] 
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
        public bool IsHovered { get; set; }
        // isStaring is used to check if the player is staring at a monk;
        // while readyToTalk is used to check if the required amount of time to make the monk talk has passed
        public static bool isStaring;
        public bool readyToTalk;
        public static float HoveringTime;
        [SerializeField] private float staringTime = 1.0f;
        [SerializeField] private float staringTimeToTalk = 2.0f;
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

            // Case where EyeInteractable instance is a monk
            //if (gameObjLayer == monkLayer) {
            //    if (TryGetComponent<AudioSource>(out var aS)) {
            //        monkAudioSource = aS;
            //    }
            //    else Debug.LogWarning("AudioSource component not found.");
            //}
            // Case where EyeInteractable instance is a square
            if (gameObjLayer == squareLayer) {
                if (TryGetComponent<MeshRenderer>(out var mR)) meshRenderer = mR;
                else Debug.LogWarning("MeshRenderer component not found.");
            } 

            if (TryGetComponent<EyeOutline>(out var eO)) {
                eyeOutline = eO;
                eyeOutline.enabled = false;
            }
            else Debug.LogWarning("EyeOutline component not found.");
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
                if (meshRenderer) meshRenderer.material = OnHoverActiveMaterial;

                // Hover ONLY for one monk at a time
                if (gameObjLayer == monkLayer && HoveringTime > staringTime) {
                    isStaring = true;
                    OutlineWidthControl();
                    VocalKeyHoldingCheck();
                    // Hover to tell the player that he can speak to the hovered monk
                    if (HoveringTime > staringTimeToTalk && VocalKeyHoldingCheck()) {
                        isStaring = false;
                        eyeOutline.OutlineWidth = maxWidthValue;
                        eyeOutline.OutlineColor = Color.green;
                        readyToTalk = true;
                        // Triggers the voice recording event 
                        EyeTrackingDebug.Instance.TriggerVoiceRecordingEvent();
                        readyToTalk = false;
                    }
                }
            } else ResetHover();
        }

        internal void ResetHover() {
            if (gameObjLayer == squareLayer && meshRenderer) meshRenderer.material = OnHoverInactiveMaterial;
            else if (gameObjLayer == monkLayer) eyeOutline.enabled = false;
        }

        private bool VocalKeyHoldingCheck() {
            bool isButtonHeld = false;
            if (OVRInput.Get(keyForVoiceControl)) {
                Debug.Log("A pressed");
                StartCoroutine(GradualControllerVibration());
                buttonHoldTime += Time.deltaTime;
                if (buttonHoldTime > staringTime + staringTimeToTalk) {
                    isButtonHeld = true;
                }
            } else buttonHoldTime = 0f;
            Debug.Log($"isButtonHeld value: {isButtonHeld}");
            return isButtonHeld;
        }

        private void OutlineWidthControl() {
            if (isStaring) {
                eyeOutline.enabled = true;
                eyeOutline.OutlineColor = Color.yellow;
                eyeOutline.OutlineMode = EyeOutline.Mode.OutlineAll;
                eyeOutline.OutlineWidth = Mathf.Lerp(0, maxWidthValue, buttonHoldTime);
            }
        }

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
    }
}