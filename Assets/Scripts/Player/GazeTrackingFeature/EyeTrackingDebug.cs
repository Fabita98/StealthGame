using Meta.WitAi.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature
{
    public class EyeTrackingDebug : MonoBehaviour
    {
        public static EyeTrackingDebug Instance { get; private set; }

        [Header("Voice recording parameters")]
        private readonly int recordingLengthInMs = 3000;
        public GameObject staredMonkForSingleton;
        public Mic oculusMic;
        public bool oculusMicFound = false;
        private readonly string directory = "Recordings";
        private bool _recording;
        private long _startTimestamp;
        private readonly List<float> samples = new();

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
            SearchForOculusMic();
            StartInvokeVoiceRecordingCoroutine();
        }

        private void OnEnable() {
            EyeInteractable.OnCounterChanged += HandleCounterChange;
            OnVoiceRecording += HandleVoiceRecording;
        }

        private void OnDisable() {
            EyeInteractable.OnCounterChanged -= HandleCounterChange;
            OnVoiceRecording -= HandleVoiceRecording;
            StopRecording();
        }

        public void TriggerVoiceRecordingEvent(AudioClip audioClip) => OnVoiceRecording?.Invoke(audioClip);

        private void HandleCounterChange(int newCount) => Debug.Log($"Current EyeInteractable instance counter: {newCount}");

        #region Voice recording 
        private void HandleVoiceRecording(AudioClip audioClip) {
            if (GazeLine.staredMonk) {
                staredMonkForSingleton = GazeLine.staredMonk;
                if (staredMonkForSingleton.TryGetComponent(out AudioSource staredMonkAudioSource)) {
                    StartCoroutine(OculusMicRecordingCoroutine(staredMonkAudioSource));
                }
                else return;
            }
        }

        private IEnumerator InvokeVoiceRecording() {
            yield return new WaitForSeconds(5f);
            OnVoiceRecording?.Invoke(null);
        }

        private void StartInvokeVoiceRecordingCoroutine() => StartCoroutine(InvokeVoiceRecording());

        #region Voice recording coroutine with Unity's Microphone API
        public IEnumerator BuiltinRecordingCoroutine(AudioSource targetAudioSource) {
            Debug.Log($"Microphone device: {Microphone.devices[0]}");
            if (Microphone.IsRecording(null)) {
                Debug.LogWarning("Microphone is already recording!");
                yield break; // Exit the coroutine if already recording
            }
            OnRecordingAboutToStart?.Invoke();

            AudioClip recordedClip = Microphone.Start(null, false, recordingLengthInMs, 44100);
            Debug.Log("Voice recording started...");

            yield return new WaitForSeconds(recordingLengthInMs);

            Microphone.End(null); // Stop the microphone recording
            Debug.Log("Voice recording stopped.");
            OnRecordingStopped?.Invoke();

            if (recordedClip != null) {
                targetAudioSource.clip = recordedClip;
                targetAudioSource.Play();
                Debug.Log("Playing recorded voice through the monk...");
            }
            else Debug.LogWarning("No recorded clip to play.");
        }        
        #endregion

        #region Voice recording coroutine with Voice SDK
        private void SearchForOculusMic() {
            Debug.Log($"Persistent data path: {Application.persistentDataPath}");
            if (!oculusMic) {
                oculusMic = FindObjectOfType<Mic>();
                oculusMic.OnSampleReady += OnSampleReady;
                if (oculusMic) {
                    oculusMicFound = true;
                    Debug.Log($"{oculusMic.CurrentDeviceName} found as Oculus Mic.");
                    oculusMic.SafeStartMicrophone();
                } 
                else Debug.LogWarning("Oculus Mic not found.");
            }            
        }

        public IEnumerator OculusMicRecordingCoroutine(AudioSource targetAudioSource) {
            if (oculusMicFound && (oculusMic.State.Equals(MicState.On) || oculusMic.State.Equals(MicState.Enabling))) {
                OnRecordingAboutToStart?.Invoke();
                //oculusMic.StartRecording(recordingLengthInMs);
                StartRecording();

                //int timeToWaitInS = recordingLengthInMs / 1000;
                //yield return new WaitForSeconds(timeToWaitInS);

                //Invoke(nameof(OculusStopRecording), timeToWaitInS);
                StopRecording();
                OnRecordingStopped?.Invoke();

                if (oculusMic.AudioClip) {
                    targetAudioSource.clip = oculusMic.AudioClip;
                    targetAudioSource.Play();
                    SavWav.Save("oculus_mic_recording", oculusMic.AudioClip);
                    Debug.Log("Playing recorded voice through Oculus Mic...");
                } else Debug.LogWarning("No recorded clip to play from Oculus Mic.");
            } 
            else Debug.LogWarning("Oculus Mic not found or not initialized.");
            yield return null;
        }

        private void OculusStopRecording() {
            Debug.Log("OculusMic Invoked with delay.");
            oculusMic.StopRecording();            
        }
        #endregion

        #region Fra&Marco NewVoiceRecorder methods
        private void OnSampleReady(int sampleCount, float[] sample, float levelMax) {
            //Debug.Log($"Sample ready: {sample.Length} samples");
            samples.AddRange(sample);
        }

        public void StartRecording() {
            if (_recording) {
                Debug.LogWarning("Trying to start recording while already recording");
                return;
            }
            _startTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            samples.Clear();
            _recording = true;
        }

        public void StopRecording(string label = "") {
            if (!_recording) {
                Debug.LogWarning("Trying to stop recording while not recording");
                return;
            }
            if (samples.Count == 0) {
                Debug.LogWarning("Trying to stop recording with no samples");
                _recording = false;
                return;
            }
            var endTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var clip = AudioClip.Create("Recording", samples.Count, 1, 16000, false);
            clip.SetData(samples.ToArray(), 0);
            _recording = false;
            label = label == "" ? "" : $"_{label}";
            var filename = $"{_startTimestamp}_{endTimestamp}{label}";
            SavWav.Save(directory + filename, clip);
            SavWav.Save($"{directory + filename}_trimmed.wav", clip.TrimSilence(0.01f));
        }
        #endregion
        #endregion
    }
}