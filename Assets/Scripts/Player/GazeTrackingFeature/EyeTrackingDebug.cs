using System;
using System.Collections;
using System.Linq;
using Unity.Labs.SuperScience;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class EyeTrackingDebug : MonoBehaviour {
        public static EyeTrackingDebug Instance { get; private set; }
        [Header("Voice playback variables")]
        private const float maxSnoringTime = 12f;
        private const float minSnoringTime = 4f;

        public static event SnoringAudioPlaybackHandler OnAudioPlayback;
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

        private void Start() {
            StartInvokeVoicePlaybackCoroutine();
        }

        private void OnEnable() {
            EyeInteractable.OnCounterChanged += HandleCounterChange;
            OnAudioPlayback += HandleSnoringAudioPlayback;
        }

        private void OnDisable() {
            EyeInteractable.OnCounterChanged -= HandleCounterChange;
            OnAudioPlayback -= HandleSnoringAudioPlayback;
        }

        private void HandleCounterChange(int newCount) => Debug.Log($"Current EyeInteractable instance counter: {newCount}");

        #region AudioClip playback
        private void HandleSnoringAudioPlayback() {
            if (GazeLine.staredMonk != null) {
                OnPlaybackAboutToStart?.Invoke();
                if (EyeInteractable.snoringAudio != null) StartSnoringAudioCoroutine();
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

        public void TriggerVoiceRecordingEvent() => OnAudioPlayback?.Invoke();

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
        #endregion

        #region NO headset usage
        /// <summary>
        /// Coroutine and invokeCoroutine to invoke without headset usage
        /// </summary>
        private IEnumerator InvokeVoicePlayback() {
            if (GazeLine.staredMonk != null) {
                OnAudioPlayback?.Invoke();
            }
            else {
                Debug.LogError("staredMonk is null ");
                yield break;
            }
            yield return new WaitForSeconds(3f);
        }

        private void StartInvokeVoicePlaybackCoroutine() => StartCoroutine(InvokeVoicePlayback());
        #endregion
        #endregion
    }
}