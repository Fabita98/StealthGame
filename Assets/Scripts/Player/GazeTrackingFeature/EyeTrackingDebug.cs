using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class EyeTrackingDebug : MonoBehaviour
    {
        public static EyeTrackingDebug Instance { get; private set; }

        [Header("Voice recording parameters")]
        public EyeInteractable staredMonkForSingleton;
        [SerializeField] private AudioClip monkAudioClip;
        public static event VoiceRecordingHandler OnVoiceRecording;
        public delegate void VoiceRecordingHandler(AudioClip audioClip);
        // Used to enable/disable the speaking text UI
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

        private void Start() {
            StartInvokeVoiceRecordingCoroutine();
        }

        private void OnEnable() {
            EyeInteractable.OnCounterChanged += HandleCounterChange;
            OnVoiceRecording += HandleVoiceRecording;
        }

        private void OnDisable() {
            EyeInteractable.OnCounterChanged -= HandleCounterChange;
            OnVoiceRecording -= HandleVoiceRecording;
        }

        public void TriggerVoiceRecordingEvent() => OnVoiceRecording?.Invoke(null);

        private void HandleCounterChange(int newCount) => Debug.Log($"Current EyeInteractable instance counter: {newCount}");

        #region AudioClip playback
        private void HandleVoiceRecording(AudioClip aC) {
            if (GazeLine.staredMonk) {
                staredMonkForSingleton = GazeLine.staredMonk;
                if (staredMonkForSingleton.TryGetComponent(out AudioSource staredMonkAudioSource)) {
                    aC = monkAudioClip;
                    staredMonkAudioSource.clip = aC;
                }
                else return;
            }
        }

        private IEnumerator InvokeVoiceRecording() {
            yield return new WaitForSeconds(3f);
            OnVoiceRecording?.Invoke(null);
        }

        // Coroutine to invoke without headset usage
        private void StartInvokeVoiceRecordingCoroutine() => StartCoroutine(InvokeVoiceRecording());
        #endregion
    }
}