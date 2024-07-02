using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature
{
    public class EyeTrackingDebug : MonoBehaviour
    {
        public static EyeTrackingDebug Instance { get; private set; }

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

        private void OnEnable()
        {
            EyeInteractable.OnCounterChanged += HandleCounterChange;
            EyeInteractable.OnVoiceRecording += HandleVoiceRecording;
        }

        private void OnDisable()
        {
            EyeInteractable.OnCounterChanged -= HandleCounterChange;
            EyeInteractable.OnVoiceRecording -= HandleVoiceRecording;
        }

        private void HandleCounterChange(int newCount)
        {
            Debug.Log($"Current EyeInteractable instance counter: {newCount}");
        }

        private void HandleVoiceRecording(AudioClip audioClip)
        {


        }

        private void StartVoiceCoroutine()
        {
            AudioSource monkAudioSource = EyeInteractable.audioSource;

        }
    }
}