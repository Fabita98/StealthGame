using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class EyeTrackingDebug : MonoBehaviour
    {
        public static EyeTrackingDebug Instance { get; private set; }
        [Header("Voice playback variables")]
        public EyeInteractable staredMonkForSingleton;
        private const string audioDataPath = "C:\\Users\\utente\\Desktop\\Unity projects\\StealthGame\\Assets\\Art\\Audio\\AudioPool";
        private float maxSnoringTime = 10f;
        private float minSnoringTime = 1f;

        public static event VoiceRecordingHandler OnVoiceRecording;
        public delegate void VoiceRecordingHandler();
        /// <summary>
        /// Events used to enable/disable the speaking text UI
        /// </summary>
        public static event Action OnRecordingAboutToStart;
        public static event Action OnRecordingStopped;

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
                staredMonkForSingleton = GazeLine.staredMonk;
                OnRecordingAboutToStart?.Invoke();
                if (staredMonkForSingleton.TryGetComponent(out AudioSource staredMonkAudioSource)) {
                    StartCoroutine(WaitForAudioToEnd(maxSnoringTime));
                    Debug.Log("Monk is talking now with " + staredMonkAudioSource.clip.name);
                }
                else return;
            }
        }
        
        private IEnumerator WaitForAudioToEnd(float duration) {
            if (staredMonkForSingleton.TryGetComponent<AudioSource>(out var monkAudioSource)) {
                staredMonkForSingleton.StartExplosiveVibration();
                staredMonkForSingleton.snoringAudio.Play();
                yield return new WaitForSeconds(duration);
                OnRecordingStopped?.Invoke();
                staredMonkForSingleton.StopExplosiveVibration();
            } else yield break;           
        }

        #region Snoring audio playback
        public void StartSnoringAudioCoroutine() => StartCoroutine(PlaySnoringAudioCoroutine());
        
        /// <summary>
        /// Coroutine to play snoring audio depending on stress value. Stress value is temporary and will be replaced with a more complex system.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="stressValue"></param>
        /// <returns></returns>
        private IEnumerator PlaySnoringAudioCoroutine(float stressValue = .5f) {
            float duration = UnityEngine.Random.Range(minSnoringTime, maxSnoringTime);
            if (staredMonkForSingleton.snoringAudio.clip) {
                staredMonkForSingleton.snoringAudio.Play();
                yield return new WaitForSeconds(duration);
                staredMonkForSingleton.snoringAudio.Stop();
            }
            else {
                Debug.LogWarning("AudioSource component not found on staredMonkForSingleton.");
            }
        }
        #endregion

        public void TriggerVoiceRecordingEvent() => OnVoiceRecording?.Invoke();

        /// <summary>
        /// Coroutine and invokeCoroutine to invoke without headset usage
        /// </summary>
        private IEnumerator InvokeVoiceRecording() {
            yield return new WaitForSeconds(3f);
            OnVoiceRecording?.Invoke();
        }

        private void StartInvokeVoiceRecordingCoroutine() => StartCoroutine(InvokeVoiceRecording());
        #endregion
    }
}