using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class EyeTrackingDebug : MonoBehaviour
    {
        public static EyeTrackingDebug Instance { get; private set; }
        [Header("Voice playback variables")]
        private const string audioDataPath = "C:\\Users\\utente\\Desktop\\Unity projects\\StealthGame\\Assets\\Art\\Audio\\AudioPool";
        private const float maxSnoringTime = 12f;
        private const float minSnoringTime = 4f;

        public static event VoiceRecordingHandler OnVoiceRecording;
        public delegate void VoiceRecordingHandler();
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
        //    StartInvokeVoiceRecordingCoroutine();
        //}

        private void OnEnable() {
            EyeInteractable.OnCounterChanged += HandleCounterChange;
            OnVoiceRecording += HandleVoiceRecording;
        }

        private void OnDisable() {
            EyeInteractable.OnCounterChanged -= HandleCounterChange;
            OnVoiceRecording -= HandleVoiceRecording;
        }

        private void HandleCounterChange(int newCount) => Debug.Log($"Current EyeInteractable instance counter: {newCount}");
        
        #region AudioClip playback
        private void HandleVoiceRecording() {
            if (GazeLine.staredMonk) {
                OnPlaybackAboutToStart?.Invoke();
                if (GazeLine.staredMonk.snoringAudio) {
                    StartSnoringAudioCoroutine();
                }
                else { 
                    Debug.LogWarning("snoringAudio not found on staredMonkForSingleton -> SnoringCoroutine not launched! ");
                    return; 
                }
            }
        }

        public void TriggerVoiceRecordingEvent() => OnVoiceRecording?.Invoke();

            #region Snoring audio playback        
        /// <summary>
        /// Coroutine to play snoring audio depending on stress value. Stress value is temporary and will be replaced with a more complex system.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="stressValue"></param>
        /// <returns></returns>
        private IEnumerator PlaySnoringAudioCoroutine(float stressValue = .5f) {
            float duration = UnityEngine.Random.Range(minSnoringTime, maxSnoringTime);
            if (GazeLine.staredMonk.snoringAudio.clip) {
                GazeLine.staredMonk.snoringAudio.Play();
                yield return new WaitForSeconds(duration);
                GazeLine.staredMonk.snoringAudio.Stop();
            }
            else {
                Debug.LogWarning("AudioSource component not found on staredMonkForSingleton.");
                yield break;
            }
        }

        public void StartSnoringAudioCoroutine() => StartCoroutine(PlaySnoringAudioCoroutine());
        #endregion

            #region NO headset usage
        /// <summary>
        /// Coroutine and invokeCoroutine to invoke without headset usage
        /// </summary>
        //private IEnumerator InvokeVoiceRecording() {
        //    yield return new WaitForSeconds(3f);
        //    OnVoiceRecording?.Invoke();
        //}

        //private void StartInvokeVoiceRecordingCoroutine() => StartCoroutine(InvokeVoiceRecording());
        #endregion
        #endregion
    }
}