using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

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

        [Header("Voice playback variables")]
        [SerializeField] private const float maxSnoringTime = 10f;
        [SerializeField] private const float minSnoringTime = 4f;
        internal static float snoringCooldownCurrentTime;
        internal static bool isVocalPowerActive;

        [Header("Vibration")]
        private readonly OVRInput.RawButton keyForVoiceControl = OVRInput.RawButton.B;
        public static float buttonHoldTime;
        private bool isVibrating;

        // Event to be invoked to debug logger, if needed
        [SerializeField] private UnityEvent<GameObject> OnObjectHover;

        public static event SnoringAudioPlaybackHandler OnSnoringAudioPlayback;
        public delegate void SnoringAudioPlaybackHandler();
        /// <summary>
        /// Events used to enable/disable the speaking text UI
        /// </summary>
        public static event Action OnPlaybackAboutToStart;
        public static event Action OnPlaybackStopped;
        private void HandleEyeInteractableInstancesCounterChange(int newCount) => Debug.Log($"Current EyeInteractable instance counter: {newCount}");
        public bool HandlePinkLotusPowerActivation(bool isActive) => isVocalPowerActive = isActive;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this) {
                Destroy(this);
            }
        }

        private void OnEnable() {
            EyeInteractable.OnEyeInteractableInstancesCounterChanged += HandleEyeInteractableInstancesCounterChange;
            OnSnoringAudioPlayback += HandleSnoringAudioPlayback;
            Flower_animator_mindcontrol.OnPinkLotusPowerChanged += HandlePinkLotusPowerActivation;
        }

        private void OnDisable() {
            EyeInteractable.OnEyeInteractableInstancesCounterChanged -= HandleEyeInteractableInstancesCounterChange;
            OnSnoringAudioPlayback -= HandleSnoringAudioPlayback;
            Flower_animator_mindcontrol.OnPinkLotusPowerChanged -= HandlePinkLotusPowerActivation;
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
        }

        #region Gaze features methods
        public void GazeControl() {
            if (GazeLine.staredMonk != null && GazeLine.staredMonk.IsHovered)
            {
                BaseStateMachine enemyStateMachine = GazeLine.staredMonk.gameObject.GetComponent<BaseStateMachine>();
                if(enemyStateMachine.CurrentState.name != "StatePatrol") return;
                OnObjectHover?.Invoke(gameObject);
                // Case 1: Hovering monk -> blue outline
                OutlineWidthControl(Color.white); //blue

                // Case 1.1 : Keep staring at the monk -> switch to yellow outline && Vocal Key enabled
                if (EyeInteractable.HoveringTime > staringTimeToPressVocalKey) {
                    GazeLine.staredMonk.isBeingStared = true;
                    OutlineWidthControl(Color.white); //yellow
                    GazeLine.staredMonk.gameObject.GetComponent<EnemyUtility>().ableToSleepButtonUI.SetActive(true);
                    // Case 1.1.1 : Keep staring at the monk after having both stared at it
                    // and held the key for required amount of time -> switch to green outline && starts snoring
                    if (EyeInteractable.HoveringTime > staringTimeToTalk && VocalKeyHoldingCheck()) {
                        GazeLine.staredMonk.isBeingStared = false;
                        GazeLine.staredMonk.readyToTalk = true;
                        StartRightControllerStrongVibrationCoroutine();
                        OutlineWidthControl(Color.grey); //green
                        GazeLine.staredMonk.gameObject.GetComponent<EnemyUtility>().ableToSleepButtonUI.SetActive(false);
                        if (EyeInteractable.snoringAudio != null) EyeTrackingDebug.SnoringAudioPlaybackTrigger();
                        else Debug.LogWarning("snoringAudio is null -> Event not triggered!");
                        GazeLine.staredMonk.readyToTalk = false;
                    }
                }
            }
            else return;
        }

        private bool VocalKeyHoldingCheck() {
            if (OVRInput.Get(keyForVoiceControl) && GazeLine.staredMonk.isBeingStared) {
                StartCoroutine(GradualControllerVibration());
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
    }
}