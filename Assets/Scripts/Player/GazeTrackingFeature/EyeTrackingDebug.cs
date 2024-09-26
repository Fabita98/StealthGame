using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    internal class EyeTrackingDebug : MonoBehaviour
    {
        public static EyeTrackingDebug Instance { get; private set; }

        [Header("Voice playback parameters")]
        public EyeInteractable staredMonkForSingleton;
        [SerializeField] private AudioClip monkAudioClip;
        [SerializeField] private GazeLine gazeLine;

        public static event VoiceRecordingHandler OnVoiceRecording;
        public delegate void VoiceRecordingHandler();
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
            //StartInvokeVoiceRecordingCoroutine();
        }

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
            if (gazeLine.staredMonk) {
                staredMonkForSingleton = gazeLine.staredMonk;
                OnRecordingAboutToStart?.Invoke();
                if (staredMonkForSingleton.TryGetComponent(out AudioSource staredMonkAudioSource)) {
                    staredMonkForSingleton.StartExplosiveVibration();
                    staredMonkAudioSource.Play();
                    Debug.Log("Monk is talking now with " + staredMonkAudioSource.clip.name);
                    StartCoroutine(WaitForAudioToEnd(staredMonkAudioSource));
                }
                else return;
            }
        }
        
        private IEnumerator WaitForAudioToEnd(AudioSource audioSource) {
            yield return new WaitWhile(() => audioSource.isPlaying);
            OnRecordingStopped?.Invoke();
            staredMonkForSingleton.ResetHover();
            staredMonkForSingleton.StopExplosiveVibration();
        }
        
        private IEnumerator InvokeVoiceRecording() {
            yield return new WaitForSeconds(3f);
            OnVoiceRecording?.Invoke();
        }
        
        public void TriggerVoiceRecordingEvent() => OnVoiceRecording?.Invoke();

        // Coroutine to invoke without headset usage
        private void StartInvokeVoiceRecordingCoroutine() => StartCoroutine(InvokeVoiceRecording());
        #endregion
    }
}