using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature
{
    public class EyeTrackingDebug : MonoBehaviour
    {
        public static EyeTrackingDebug Instance { get; private set; }

        [Header("Voice recording parameters")]
        [SerializeField] private int recordingLength = 3;
        public static AudioSource audioSource;
        [SerializeField] public GameObject trialMonk;
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
            if (trialMonk == null) trialMonk = GameObject.FindGameObjectWithTag("Monk");
            // wait 5s than invoke event
            StartCoroutine(InvokeVoiceRecording());
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

        private void HandleCounterChange(int newCount)
        {
            Debug.Log($"Current EyeInteractable instance counter: {newCount}");
        }

        #region Voice recording 
        #region Voice recording coroutine with Unity's Microphone API
        private void HandleVoiceRecording(AudioClip audioClip)
        {
            GazeLine.staredMonk = trialMonk;

            if (GazeLine.staredMonk && GazeLine.staredMonk.TryGetComponent<AudioSource>(out var monkAudioSource))
            {
                StartCoroutine(VoiceRecordingCoroutine(monkAudioSource));
            }
            else
            {
                if (trialMonk.TryGetComponent<AudioSource>(out var trialMonkAudioSource))
                StartCoroutine(VoiceRecordingCoroutine(trialMonkAudioSource));
            }
        }

        public IEnumerator VoiceRecordingCoroutine(AudioSource targetAudioSource)
        {
            if (Microphone.IsRecording(null))
            {
                Debug.LogWarning("Microphone is already recording!");
                yield break; // Exit the coroutine if already recording
            }
            OnRecordingAboutToStart?.Invoke();

            AudioClip recordedClip = Microphone.Start(null, false, recordingLength, 44100); 
            Debug.Log($"Microphone device: {Microphone.devices[0]}");
            Debug.Log("Voice recording started...");

            yield return new WaitForSeconds(3);

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

        //        private void CollectLipSync()
        //        {
        //            var frame = lipsyncTracker.GetLastProcessedFrame();

        //            List<float> visemesWheights = new(frame.Visemes);

        //            if (visemesWheights != null)
        //            {
        //                foreach (var viseme in (OVRLipSync.Viseme[])Enum.GetValues(typeof(OVRLipSync.Viseme)))
        //                {
        //                    AddToDictionary(viseme.ToString(), visemesWheights[(int)viseme].ToString());
        //                }

        //                AddToDictionary("Laughter probability", frame.laughterScore.ToString());
        //            }
        //            else
        //            {
        //                foreach (var viseme in (OVRLipSync.Viseme[])Enum.GetValues(typeof(OVRLipSync.Viseme)))
        //                {
        //                    AddToDictionary(viseme.ToString(), "NaN", false);
        //                }

        //                AddToDictionary("Laughter probability", "NaN", false);
        //            }
        //        }

        //        public void CreateLipsyncTracker()
        //        {
        //            GameObject lipsynctrackerObj = new("LipsyncTracker", typeof(LipSyncTracker));
        //            lipsynctrackerObj.transform.parent = transform;

        //            var lipsyncTrackerTemp = lipsynctrackerObj.GetComponent<LipSyncTracker>();
        //            lipsyncTrackerTemp.provider = OVRLipSync.ContextProviders.Enhanced_with_Laughter;
        //            lipsyncTrackerTemp.audioLoopback = false;
        //            lipsyncTrackerTemp.micSelected = Microphone.devices[0];
        //        }

        //        public void RemoveLipsyncTracker()
        //        {
        //#if UNITY_EDITOR
        //            EditorApplication.delayCall += () => {
        //                Transform lipTransform = null;
        //                if (this != null && transform != null)
        //                    lipTransform = transform.Find("LipsyncTracker");

        //                if (lipTransform)
        //                {
        //                    DestroyImmediate(lipTransform.gameObject);
        //                }
        //            };
        //#endif
        //        }

        #endregion
        #endregion
    }
}