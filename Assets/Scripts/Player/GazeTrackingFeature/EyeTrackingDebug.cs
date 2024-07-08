using Meta.WitAi.Lib;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature
{
    public class EyeTrackingDebug : MonoBehaviour
    {
        public static EyeTrackingDebug Instance { get; private set; }

        [Header("Voice recording parameters")]
        [SerializeField] private int recordingLengthInMS = 30000;
        public GameObject trialMonk;
        public Mic oculusMic;
        public bool oculusMicFound = false;

        public static event VoiceRecordingHandler OnVoiceRecording;
        public delegate void VoiceRecordingHandler(AudioClip audioClip);
        public static event Action OnRecordingAboutToStart;
        public static event Action OnRecordingStopped;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this)
            {
                Destroy(this); 
            }
        }

        private void Start()
        {
            SearchForOculusMic();
        }

        private void OnEnable()
        {
            EyeInteractable.OnCounterChanged += HandleCounterChange;
            OnVoiceRecording += HandleVoiceRecording;
        }

        private void OnDisable()
        {
            EyeInteractable.OnCounterChanged -= HandleCounterChange;
            OnVoiceRecording -= HandleVoiceRecording;
        }

        public void TriggerVoiceRecordingEvent(AudioClip audioClip)
        {
            OnVoiceRecording?.Invoke(audioClip);
        }

        private void HandleCounterChange(int newCount)
        {
            Debug.Log($"Current EyeInteractable instance counter: {newCount}");
        }

        #region Voice recording 
        private void HandleVoiceRecording(AudioClip audioClip)
        {
            //GazeLine.staredMonk = trialMonk;
            trialMonk = GazeLine.staredMonk;

            if (trialMonk && trialMonk.TryGetComponent<AudioSource>(out var monkAudioSource))
            {
                StartCoroutine(OculusMicRecordingCoroutine(monkAudioSource));
            }
            else
            {
                return;
                //if (trialMonk.TryGetComponent<AudioSource>(out var trialMonkAudioSource))
                //StartCoroutine(VoiceRecordingCoroutine(trialMonkAudioSource));
            }
        }        

        #region Voice recording coroutine with Unity's Microphone API
        public IEnumerator BuiltinRecordingCoroutine(AudioSource targetAudioSource)
        {
            Debug.Log($"Microphone device: {Microphone.devices[0]}");
            if (Microphone.IsRecording(null))
            {
                Debug.LogWarning("Microphone is already recording!");
                yield break; // Exit the coroutine if already recording
            }
            OnRecordingAboutToStart?.Invoke();

            AudioClip recordedClip = Microphone.Start(null, false, recordingLengthInMS, 44100);
            Debug.Log("Voice recording started...");

            yield return new WaitForSeconds(recordingLengthInMS);

            Microphone.End(null); // Stop the microphone recording
            Debug.Log("Voice recording stopped.");
            OnRecordingStopped?.Invoke();

            if (recordedClip != null)
            {
                targetAudioSource.clip = recordedClip;
                targetAudioSource.Play();
                Debug.Log("Playing recorded voice through the monk...");
            }
            else Debug.LogWarning("No recorded clip to play.");
        }

        private IEnumerator InvokeVoiceRecording()
        {
            yield return new WaitForSeconds(5f);
            OnVoiceRecording?.Invoke(null);
        }
        #endregion

        #region Voice recording coroutine with Voice SDK

        private void SearchForOculusMic()
        {
            Debug.Log($"Persistent data path: {Application.persistentDataPath}");

            if (oculusMic == null) {
                oculusMic = FindObjectOfType<Mic>();
                if (oculusMic != null) {
                    oculusMicFound = true;
                    Debug.Log("Oculus Mic found.");
                } 
                else Debug.LogWarning("Oculus Mic not found.");
            }            
        }

        public IEnumerator OculusMicRecordingCoroutine(AudioSource targetAudioSource) {
            if (oculusMicFound && oculusMic != null) {
                OnRecordingAboutToStart?.Invoke();
                oculusMic.StartRecording(recordingLengthInMS);

                yield return new WaitForSeconds(recordingLengthInMS);

                oculusMic.StopRecording();
                OnRecordingStopped?.Invoke();

                if (oculusMic.AudioClip != null) {
                    targetAudioSource.clip = oculusMic.AudioClip;
                    SavWav.Save("oculus_mic_recording", oculusMic.AudioClip);
                    targetAudioSource.Play();
                    Debug.Log("Playing recorded voice through Oculus Mic...");
                } else Debug.LogWarning("No recorded clip to play from Oculus Mic.");
            } 
            else Debug.LogWarning("Oculus Mic not found or not initialized.");
        }
        #endregion
        #endregion
    }
}