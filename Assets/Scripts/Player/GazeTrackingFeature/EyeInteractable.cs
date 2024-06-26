using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.GazeTrackingFeature
{
    [RequireComponent(typeof(Rigidbody))]
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
        public float HoveringTime { get; set; }

        [Header("Game object layer")]
        private LayerMask gameObjLayer;  
        private int squareLayer, monkLayer;

        [Header("Voice recording settings")]
        [SerializeField] private int recordingLength = 3; 
        private bool isRecording = false;
        private AudioClip recordedClip;
        private AudioSource audioSource;

        public static int OverallEyeInteractableInstanceCounter { get; private set; }
        public static event CounterChangeHandler OnCounterChanged;
        public delegate void CounterChangeHandler(int newCount);
        #endregion

        void Awake()
        {            
            ComponentInit();
        }

        private void FixedUpdate()
        {
            if (IsHovered) {
                if (meshRenderer) meshRenderer.material = OnHoverActiveMaterial;
                else GazeControl();
                OnObjectHover?.Invoke(gameObject);
            }
            else
            {
                ResetHover();
            }
        }

        private void ComponentInit() {
            gameObjLayer = gameObject.layer;
            monkLayer = LayerMask.NameToLayer("Monks");
            squareLayer = LayerMask.NameToLayer("Squares");

            // Case where EyeInteractable instance is a monk 
            if (gameObjLayer == monkLayer) {
                if (TryGetComponent<AudioSource>(out var aS)) audioSource = aS;
                else Debug.LogWarning("AudioSource component not found.");
            }
            // Case where EyeInteractable instance is a square
            else if (gameObjLayer == squareLayer) {
                if (TryGetComponent<MeshRenderer>(out var mR)) meshRenderer = mR;
                else Debug.LogWarning("MeshRenderer component not found.");
            } 

            if (TryGetComponent<EyeOutline>(out var eO))
            {
                eyeOutline = eO;
                eyeOutline.enabled = true;
                ResetHover();
            }
            else Debug.LogWarning("EyeOutline component not found.");
        }

        #region EyeInteractable instances counter
        void OnEnable()
        {
            OverallEyeInteractableInstanceCounter++;
            OnCounterChanged?.Invoke(OverallEyeInteractableInstanceCounter);
        }

        void OnDisable()
        {
            OverallEyeInteractableInstanceCounter--;
            OnCounterChanged?.Invoke(OverallEyeInteractableInstanceCounter);
        }
        #endregion

        #region Eye and voice control methods

            #region Eye control methods
            public void ResetHover()
        {
            HoveringTime = 0;
            IsHovered = false;
            if (gameObjLayer == squareLayer && meshRenderer) meshRenderer.material = OnHoverInactiveMaterial; 
            else eyeOutline.OutlineWidth = 0;
        }

            public void GazeControl()
        {
            HoveringTime += Time.fixedDeltaTime;
            // Hover for one monk at a time
            if (HoveringTime >= 1f) {
                eyeOutline.OutlineColor = Color.yellow;
                eyeOutline.OutlineMode = EyeOutline.Mode.OutlineAll;
                eyeOutlineWidth += Time.fixedDeltaTime;
                Mathf.Clamp(eyeOutlineWidth, 0.2f, 3f);
                // hover to tell the player that he can speak to the hovered monk
                if (HoveringTime >= 2f) {
                    //StartVoiceRecording();
                    eyeOutlineWidth = 5;
                    eyeOutline.OutlineColor = Color.green;
                    Debug.Log("Enemy selected: ready to talk! ");
                    //StopVoiceRecording();
                    //PlayRecordedVoice();
                    ResetHover();
                }
            }
        }
            #endregion

            #region Voice recording methods
            public void StartVoiceRecording()
        {
            if (Microphone.IsRecording(null)) {
                Debug.LogWarning("Microphone is already recording!");
                return;
            }

            isRecording = true;
            recordedClip = Microphone.Start(null, false, recordingLength, 44100);
            Debug.Log("Voice recording started...");
        }

            public void StopVoiceRecording()
        {
            if (!isRecording) {
                Debug.LogWarning("Microphone is not recording.");
                return;
            }

            Microphone.End(null);
            isRecording = false;
            Debug.Log("Voice recording stopped.");
        }

            private void PlayRecordedVoice()
        {
            if (recordedClip != null)
            {
                audioSource.clip = recordedClip;
                audioSource.Play();
                Debug.Log("Playing recorded voice...");
            }
            else
            {
                Debug.LogWarning("No recorded clip to play.");
                return;
            }
        }
            #endregion
        #endregion
    }
}