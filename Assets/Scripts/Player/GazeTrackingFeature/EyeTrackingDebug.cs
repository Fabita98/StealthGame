using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class EyeTrackingDebug : MonoBehaviour {
        public static EyeTrackingDebug Instance { get; private set; }
        [Header("Voice playback variables")]
        [SerializeField] private float maxSnoringTime = 10f;
        [SerializeField] private float minSnoringTime = 4f;
        internal static bool isVocalPowerActive = false;

        public static event SnoringAudioPlaybackHandler OnSnoringAudioPlayback;
        public delegate void SnoringAudioPlaybackHandler();
        /// <summary>
        /// Events used to enable/disable the speaking text UI
        /// </summary>
        public static event Action OnPlaybackAboutToStart;
        public static event Action OnPlaybackStopped;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this) {
                Destroy(this);
            }
        }

        //private void Start() {
        //    StartInvokeVoicePlaybackCoroutine();
        //}

        private void OnEnable() {
            EyeInteractable.OnEyeInteractableInstancesCounterChanged += HandleEyeInteractableInstancesCounterChange;
            OnSnoringAudioPlayback += HandleSnoringAudioPlayback;
        }

        private void OnDisable() {
            EyeInteractable.OnEyeInteractableInstancesCounterChanged -= HandleEyeInteractableInstancesCounterChange;
            OnSnoringAudioPlayback -= HandleSnoringAudioPlayback;
        }

        private void HandleEyeInteractableInstancesCounterChange(int newCount) => Debug.Log($"Current EyeInteractable instance counter: {newCount}");

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
                Debug.LogError("staredMonk is null ");
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
                yield return new WaitForSeconds(duration);
                GazeLine.staredMonk.audioSources[0].Stop();
            }
            else {
                Debug.LogWarning("AudioSource[0] is null on staredMonkForSingleton.");
                yield break;
            }
        }

        public void StartSnoringAudioCoroutine() => StartCoroutine(PlaySnoringAudioCoroutine());

        internal void StartSnoringCooldown() {
            float snoringAudioLength = EyeInteractable.snoringAudio != null ? EyeInteractable.snoringAudio.length : 0;
            EyeInteractable.snoringCooldownEndTime = Time.time + Mathf.Max(15f, snoringAudioLength);
        }

        private void DecreasePinkPowerCounter() {
            int currentPinkLotusCounterValue = PlayerPrefsManager.GetInt(PlayerPrefsKeys.PinkLotus, 0);
            if (currentPinkLotusCounterValue > 0) {
                PlayerPrefsManager.SetInt(PlayerPrefsKeys.PinkLotus, currentPinkLotusCounterValue - 1);
                Debug.Log("PinkLotus counter value: " + currentPinkLotusCounterValue);
                UIController.Instance.AbilitiesUI.SetAbilitiesCount();
                if (currentPinkLotusCounterValue == 0) 
                    Flower_animator_wallpower.TriggerOnPinkLotusPowerChangeEvent(false);
                else
                    Flower_animator_wallpower.TriggerOnPinkLotusPowerChangeEvent(true);
            }
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
                Debug.LogError("staredMonk is null ");
                yield break;
            }
            yield return new WaitForSeconds(3f);
        }

        private void StartInvokeVoicePlaybackCoroutine() => StartCoroutine(InvokeSnoringAudioPlaybackCoroutine());
        #endregion
        #endregion
    }
}