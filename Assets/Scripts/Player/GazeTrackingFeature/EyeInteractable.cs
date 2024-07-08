using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.GazeTrackingFeature
{
    [RequireComponent(typeof(EyeOutline))]
    [RequireComponent(typeof(AudioSource))] 
    internal class EyeInteractable : MonoBehaviour
    {
        #region Variables and events definition
        [Header("Eye hovering parameters")]
        [SerializeField] private Material OnHoverActiveMaterial;
        [SerializeField] private Material OnHoverInactiveMaterial;
        // Event to be invoked to debug logger, if needed
        [SerializeField] private UnityEvent<GameObject> OnObjectHover;
        private EyeOutline eyeOutline;
        private MeshRenderer meshRenderer;
        private float eyeOutlineWidth;
        public bool IsHovered { get; set; }
        #region Make the monk talk: bools explanation
        // isStaring is used to check if the player is staring at a monk,
        // then readyToTalk is used to check if the required amount of time to make the monk talk has passed
        #endregion
        public static bool isStaring;
        public static float HoveringTime;

        [Header("Game object layer")]
        public LayerMask gameObjLayer;  
        private int squareLayer, monkLayer;

        public static int OverallEyeInteractableInstanceCounter { get; private set; }
        public static bool readyToTalk;
        public static event CounterChangeHandler OnCounterChanged;
        public delegate void CounterChangeHandler(int newCount);
        public static event VoiceRecordingHandler OnVoiceRecording;
        public delegate void VoiceRecordingHandler(AudioClip audioClip);
        #endregion

        void Awake()
        {            
            ComponentInit();
        }        

        private void Update()
        {
            GazeControl();
        }

        private void ComponentInit() {
            gameObjLayer = gameObject.layer;
            monkLayer = LayerMask.NameToLayer("Monks");
            squareLayer = LayerMask.NameToLayer("Squares");

            // Case where EyeInteractable instance is a monk 
            //if (gameObjLayer == monkLayer) {
            //    if (TryGetComponent<AudioSource>(out var aS)) audioSource = aS;
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
                if (gameObjLayer == monkLayer && HoveringTime > 1f) {
                    isStaring = true;
                    eyeOutline.enabled = true;
                    eyeOutline.OutlineColor = Color.yellow;
                    eyeOutline.OutlineMode = EyeOutline.Mode.OutlineAll;
                    eyeOutlineWidth += Time.deltaTime;
                    Mathf.Clamp(eyeOutlineWidth, 1f, 3f);

                    // Hover to tell the player that he can speak to the hovered monk
                    if (HoveringTime > 3f) {
                        isStaring = false;
                        readyToTalk = true;
                        eyeOutlineWidth = 4f;
                        eyeOutline.OutlineColor = Color.green;
                        // Trigger the voice recording event here
                        EyeTrackingDebug.Instance.TriggerVoiceRecordingEvent(null); // Passing null for now, will handle recording in the listener
                        //HoveringTime = 0f;
                        readyToTalk = false;
                    }
                }
            } else ResetHover();
        }

        public void ResetHover() {
            if (gameObjLayer == squareLayer && meshRenderer) meshRenderer.material = OnHoverInactiveMaterial;
            else if (gameObjLayer == monkLayer) eyeOutline.enabled = false;
        }
        #endregion
    }
}