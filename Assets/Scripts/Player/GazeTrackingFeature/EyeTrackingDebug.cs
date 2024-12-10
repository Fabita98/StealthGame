using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class EyeTrackingDebug : MonoBehaviour {
        public static EyeTrackingDebug Instance { get; private set; }
        
        [Header("Eye hovering parameters")]
        // isStaring is used to check if the player is staring at a monk after a minimum amount of time;
        // while readyToTalk is used to check if the required amount of time to make the monk snore has passed        
        [SerializeField] private const float staringTimeToPressVocalKey = .2f;
        [SerializeField] private const float staringTimeToTalk = .3f;
        private const float duration = staringTimeToPressVocalKey + staringTimeToTalk;
        private const float vibrationTime = .3f;
        internal const byte noWidthValue = 0;
        [SerializeField] private const float minWidthValue = .2f;
        [SerializeField] private const float maxWidthValue = 4;
        
        [Header("EndZoneFireStone")]
        [SerializeField] private GameObject completeGameParent;
        public GameObject endZoneFireStone;
        internal EyeInteractable fireStoneEyeInteractableComponent;
        public static bool FireStoneCanBeStared;
        internal BoxCollider[] FireStoneColliders;
        private BoxCollider collisionCollider;
        private BoxCollider eyeTrackingCollider;
        internal EyeOutline fireStoneEyeOutline;
        // GameObject to be activated when the player looks at the fire stone
        [SerializeField] private GameObject onLookActivationObject;

        [Header("Voice playback variables")]
        [SerializeField] private const float maxSnoringTime = 10f;
        [SerializeField] private const float minSnoringTime = 4f;
        public static float finalSnoringTime = 10.0f;
        internal static float snoringCooldownCurrentTime;
        internal static bool isVocalPowerActive;
        internal static bool isFirstYawning = true;

        [Header("Vibration")]
        private readonly OVRInput.RawButton keyForVoiceControl = OVRInput.RawButton.B;
        public static float buttonHoldTime;
        private bool isVibrating;

        public static event SnoringAudioPlaybackHandler OnSnoringAudioPlayback;
        public delegate void SnoringAudioPlaybackHandler();
        public static event CompleteGameHandler OnCompleteGameParentEnabled;
        public delegate void CompleteGameHandler();
        /// <summary>
        /// Events used to enable/disable the speaking text UI
        /// </summary>
        public static event Action OnPlaybackAboutToStart;
        public static event Action OnPlaybackStopped;

        private void HandleEyeInteractableInstancesCounterChange(int newCount) => Debug.Log($"Current EyeInteractable instance counter: {newCount}");
        public bool HandlePinkLotusPowerActivation(bool isActive) => isVocalPowerActive = isActive;
        public void HandleOnCompleteGameEnabled() {
            SetFireStoneLayer();
            InitializeEndFireStone();
        }
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this) {
                Destroy(this);
            }

            FireStoneCanBeStared = PlayerPrefsManager.GetBool(PlayerPrefsKeys.GotStickPower);
        }

        private void OnEnable() {
            EyeInteractable.OnEyeInteractableInstancesCounterChanged += HandleEyeInteractableInstancesCounterChange;
            OnSnoringAudioPlayback += HandleSnoringAudioPlayback;
            Flower_animator_mindcontrol.OnPinkLotusPowerChanged += HandlePinkLotusPowerActivation;
            OnCompleteGameParentEnabled += HandleOnCompleteGameEnabled;
        }

        private void OnDisable() {
            EyeInteractable.OnEyeInteractableInstancesCounterChanged -= HandleEyeInteractableInstancesCounterChange;
            OnSnoringAudioPlayback -= HandleSnoringAudioPlayback;
            Flower_animator_mindcontrol.OnPinkLotusPowerChanged -= HandlePinkLotusPowerActivation;
            OnCompleteGameParentEnabled -= HandleOnCompleteGameEnabled;
        }

        private void Start() {
            //StartInvokeVoicePlaybackCoroutine();
            int currentPinkLotusCounterValue = PlayerPrefsManager.GetInt(PlayerPrefsKeys.PinkLotus, 0);
            if (currentPinkLotusCounterValue > 0) {
                Flower_animator_mindcontrol.TriggerOnPinkLotusPowerChangeEvent(true);
            }
            else Flower_animator_mindcontrol.TriggerOnPinkLotusPowerChangeEvent(false);
        }

        private void Update() {
            if (isVocalPowerActive && HasSnoringCooldownPassed()) GazeControl();
            FireStoneGazeControl();
        }

        #region Gaze features methods
        public void GazeControl() {
            // Monks hovering case
            if (GazeLine.staredMonk != null && GazeLine.staredMonk.IsHovered) {
                BaseStateMachine enemyStateMachine = GazeLine.staredMonk.gameObject.GetComponent<BaseStateMachine>();
                if (enemyStateMachine.CurrentState.name != "StatePatrol") return;
                // Case 1: Hovering monk -> blue outline
                OutlineWidthControl(Color.white); //blue
                if (isFirstYawning) {
                    GazeLine.staredMonk.StartPlayYawnAudioCoroutine();
                    isFirstYawning = false;
                    GazeLine.staredMonk.gameObject.GetComponent<EnemyUtility>().ableToSleepButtonUI.SetActive(true);
                }

                // Case 1.1 : Keep staring at the monk -> switch to yellow outline && Vocal Key enabled
                if (EyeInteractable.HoveringTime > staringTimeToPressVocalKey) {
                    GazeLine.staredMonk.isBeingStared = true;
                    //OutlineWidthControl(Color.white); //yellow
                    // Case 1.1.1 : Keep staring at the monk after having both stared at it
                    // and held the key for required amount of time -> switch to green outline && starts snoring
                    if (EyeInteractable.HoveringTime > staringTimeToTalk && VocalKeyHoldingCheck()) {
                        GazeLine.staredMonk.isBeingStared = false;
                        GazeLine.staredMonk.readyToTalk = true;
                        GazeLine.staredMonk.gameObject.GetComponent<EnemyUtility>().ableToSleepButtonUI.SetActive(false);
                        StartRightControllerStrongVibrationCoroutine();
                        OutlineWidthControl(Color.grey); //green
                        if (EyeInteractable.snoringAudio != null) SnoringAudioPlaybackTrigger();
                        else Debug.LogWarning("snoringAudio is null -> Event not triggered!");
                        GazeLine.staredMonk.readyToTalk = false;
                    }
                }
            }
            else {
                foreach (var enemyUtility in GetComponents<EnemyUtility>())
                {
                    enemyUtility.ableToSleepButtonUI.SetActive(false);
                }
                return;
            }
        }

        private bool VocalKeyHoldingCheck() {
            if (OVRInput.Get(keyForVoiceControl) && GazeLine.staredMonk.isBeingStared) {
                //StartCoroutine(GradualControllerVibration());
                return true;
            }
            return false;
            // bool isButtonHeld = false;
            // if (OVRInput.Get(keyForVoiceControl) && GazeLine.staredMonk.isBeingStared) {
            //     StartCoroutine(GradualControllerVibration());
            //     buttonHoldTime += Time.deltaTime;
            //     if (buttonHoldTime >= duration) {
            //         isButtonHeld = true;
            //     }
            // }
            // else buttonHoldTime = 0f;
            // return isButtonHeld;
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
                true when staredMonk.IsHovered => maxWidthValue / 2,
                true when staredMonk.IsHovered && staredMonk.isBeingStared => lerpedWidth,
                true when staredMonk.IsHovered && staredMonk.readyToTalk => maxWidthValue,
                _ => noWidthValue,
            };

            staredMonk.eyeOutline.OutlineMode = EyeOutline.Mode.OutlineAll;
            staredMonk.eyeOutline.OutlineColor = color;
            staredMonk.eyeOutline.OutlineWidth = desiredWidth;
        }
        #endregion
        #region AudioClip playback
        private void HandleSnoringAudioPlayback() {
            if (GazeLine.staredMonk != null) {
                OnPlaybackAboutToStart?.Invoke();
                if (EyeInteractable.snoringAudio != null) {
                    StartSnoringAudioCoroutine();
                    StartSnoringCooldown();
                    DecreasePinkPowerCounter();
                }
                else {
                    Debug.LogError("snoringAudio not found on staredMonk -> SnoringCoroutine not launched! ");
                    return;
                }
            }
            else {
                Debug.LogWarning("staredMonk is null ");
                return;
            }
        }

        public static void SnoringAudioPlaybackTrigger() => OnSnoringAudioPlayback?.Invoke();

        #region Snoring audio playback        
        /// <summary>
        /// Coroutine to play snoring audio depending on stress value. Stress value is temporary and will be replaced with a more complex system.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="stressValue"></param>
        /// <returns></returns>
        private IEnumerator PlaySnoringAudioCoroutine(float stressValue = .5f) {
            float duration = math.lerp(minSnoringTime, maxSnoringTime, stressValue);
            finalSnoringTime = duration;
            if (GazeLine.staredMonk.audioSources[0] != null) {
                GazeLine.staredMonk.audioSources[0].Play();
                GazeLine.staredMonk.isSleeping = true;
                yield return new WaitForSeconds(duration);
                GazeLine.staredMonk.audioSources[0].Stop();
                GazeLine.staredMonk.isSleeping = false;
            }
            else {
                Debug.LogWarning("AudioSource[0] is null on staredMonkForSingleton.");
                yield break;
            }
        }

        public void StartSnoringAudioCoroutine() => StartCoroutine(PlaySnoringAudioCoroutine());

        internal void StartSnoringCooldown() {
            snoringCooldownCurrentTime = Time.time;
        }

        internal static bool HasSnoringCooldownPassed() {
            return Time.time > snoringCooldownCurrentTime + EyeInteractable.snoringCooldownEndTime;
        }

        private void DecreasePinkPowerCounter() {
            int currentPinkLotusCounterValue = PlayerPrefsManager.GetInt(PlayerPrefsKeys.PinkLotus, 0);
            if (currentPinkLotusCounterValue > 0) {
                PlayerPrefsManager.SetInt(PlayerPrefsKeys.PinkLotus, currentPinkLotusCounterValue - 1);
                Debug.Log("PinkLotus counter value: " + currentPinkLotusCounterValue);
                UIController.Instance.AbilitiesUI.SetAbilitiesCount();
                Flower_animator_mindcontrol.TriggerOnPinkLotusPowerChangeEvent(true);
            }
            else Flower_animator_mindcontrol.TriggerOnPinkLotusPowerChangeEvent(false);
        }
        #endregion

        #region NO headset usage
        /// <summary>
        /// Coroutine and invokeCoroutine to invoke without headset usage
        /// </summary>
        private IEnumerator InvokeSnoringAudioPlaybackCoroutine() {
            if (GazeLine.staredMonk != null) {
                OnSnoringAudioPlayback?.Invoke();
            }
            else {
                Debug.LogError("staredMonk is null in the snoring audio coroutine ");
                yield break;
            }
            yield return new WaitForSeconds(3f);
        }

        private void StartInvokeVoicePlaybackCoroutine() => StartCoroutine(InvokeSnoringAudioPlaybackCoroutine());
        #endregion
        #endregion

        #region Vibration 
        internal IEnumerator GradualControllerVibration() {
            if (isVibrating) yield break;
            isVibrating = true;

            float maxAmplitude = 1.0f;
            float maxFrequency = 1.0f;

            while (buttonHoldTime < vibrationTime) {
                // float amplitude = Mathf.Lerp(0, maxAmplitude, buttonHoldTime / vibrationTime);
                // float frequency = Mathf.Lerp(0, maxFrequency, buttonHoldTime / vibrationTime);
                OVRInput.SetControllerVibration(maxFrequency, maxAmplitude, OVRInput.Controller.RTouch);
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

            float vibDuration = 1.0f;
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

        #region Firestone methods
        private void SetFireStoneLayer() => endZoneFireStone.layer = LayerMask.NameToLayer("EndInteractableStone");

        public void InitializeEndFireStone() {
            if (endZoneFireStone == null) {
                Debug.LogError("EndZoneFireStone is not assigned in the inspector.");
                return;
            }

            SetFireStoneLayer();
            FireStoneColliders = endZoneFireStone.GetComponents<BoxCollider>();

            if (FireStoneColliders.Length < 2) {
                for (int i = FireStoneColliders.Length; i < 2; i++) {
                    gameObject.AddComponent<BoxCollider>();
                }
                FireStoneColliders = GetComponents<BoxCollider>();
            }

            collisionCollider = FireStoneColliders[0];
            eyeTrackingCollider = FireStoneColliders[1];
            collisionCollider.isTrigger = false;
            collisionCollider.center = new Vector3(0.0539016724f, -0.150000006f, -3.05907917f);
            collisionCollider.size = new Vector3(5.48551178f, 4.21999979f, 6.95542669f);
            eyeTrackingCollider.isTrigger = true;
            eyeTrackingCollider.center = new Vector3(0.0299999993f, -0.150000006f, -3.44000006f);
            eyeTrackingCollider.size = new Vector3(5.30999994f, 4.21999979f, 8.05833817f);

            // eyeOutline init
            if (endZoneFireStone.TryGetComponent<EyeOutline>(out var fStEyOut)) {
                fireStoneEyeOutline = fStEyOut;
                fireStoneEyeOutline.OutlineWidth = noWidthValue;
            }
            else fireStoneEyeOutline = endZoneFireStone.AddComponent<EyeOutline>();
            // EyeInteractable init
            if (endZoneFireStone.TryGetComponent<EyeInteractable>(out var fSEyeInterOutVar)) 
                fireStoneEyeInteractableComponent = fSEyeInterOutVar;
            else fireStoneEyeInteractableComponent = endZoneFireStone.AddComponent<EyeInteractable>();
        }     

        public static void CompleteGameEventTrigger() => OnCompleteGameParentEnabled?.Invoke();

        internal void OutlineFireStoneWidthControl(Color color) {
            bool active = FireStoneCanBeStared;
            if (active.Equals(false)) return;
            
            float lerpedWidth = Mathf.Lerp(minWidthValue, maxWidthValue, buttonHoldTime);
            float desiredWidth = active switch {
                true when fireStoneEyeInteractableComponent.IsHovered => maxWidthValue / 2,
                true when fireStoneEyeInteractableComponent.IsHovered && fireStoneEyeInteractableComponent.isBeingStared => lerpedWidth,
                true when fireStoneEyeInteractableComponent.IsHovered && fireStoneEyeInteractableComponent.readyToTalk => maxWidthValue,
                _ => noWidthValue,
            };

            if (endZoneFireStone != null) {
                fireStoneEyeOutline.OutlineMode = EyeOutline.Mode.OutlineAll;
                fireStoneEyeOutline.OutlineColor = color;
                fireStoneEyeOutline.OutlineWidth = desiredWidth;
            }            
            else Debug.LogWarning("EyeOutline component is not assigned to Firestone.");
        }

        private bool FireStoneKeyHoldingCheck() {
            if (OVRInput.Get(keyForVoiceControl) && fireStoneEyeInteractableComponent.IsHovered) {
                return true;
            }
            return false;
        }

        private void FireStoneGazeControl() {
            if (FireStoneCanBeStared) {
                if (endZoneFireStone != null) {
                    if (endZoneFireStone.TryGetComponent<EyeInteractable>(out var fsEyeInterComp)) {
                        if (fsEyeInterComp.IsHovered) {
                            OutlineFireStoneWidthControl(Color.white);
                            endZoneFireStone.GetComponent<FirestoneUI>().EnableSleepButton();
                            if (FireStoneKeyHoldingCheck()) {
                                OutlineFireStoneWidthControl(Color.gray);
                                // run method to activate firestoneGameObj effect
                                endZoneFireStone.GetComponent<FirestoneUI>().DisableSleepButton();
                            }
                        }
                    }
                    else Debug.LogWarning("FireStoneGazeControl(): EyeInteractable component not found on FireStone.");
                }
            }
        }
        #endregion
    }
}