using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    public class AudioHolder : MonoBehaviour {

        [Header("WAV audio pool for player detection")]
        internal static AudioHolder instance;
        // Paths within the Resources folder
        private const string relativeAudioPoolPath = "AudioPool"; 
        private const string relativeSnoringAudioPath = "Snoring"; 
        private List<AudioClip> audioClips;
        public GameObject audioHolder;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else if (instance != this) {
                Destroy(this);
            }
        }

        private void Start() {
            FillAudioClipList();
            AssignSnoringAudio();
        }

        internal void FillAudioClipList() {
            Debug.Log("FillAudioClipList launched ");
            if (audioHolder == null) {
                audioHolder = new GameObject("AudioHolder");
            }

            // Load all audio clips from the Resources folder
            AudioClip[] loadedAudioClips = Resources.LoadAll<AudioClip>(relativeAudioPoolPath);

            // Check if there are any audio clips found
            if (loadedAudioClips.Length > 0) {
                // Add AudioSource components for each audio clip and store the clips in the list
                audioClips = new(loadedAudioClips);
                foreach (AudioClip audioClip in loadedAudioClips) {
                    AudioSource audioSource = audioHolder.AddComponent<AudioSource>();
                    audioSource.clip = audioClip;
                }
            }
            else {
                Debug.LogWarning("No audio clips found in the specified Resources folder.");
            }
        }

        internal AudioClip AssignRandomPlayerSpottedAudio() {
            Debug.Log("AssignRandomPlayerSpottedAudio launched ");
            // Check if there are any audio clips in the list
            if (audioClips != null && audioClips.Count > 0) {
                // Randomly select one of the audio clips
                int randomIndex = Random.Range(0, audioClips.Count);
                return audioClips[randomIndex];
            }
            else {
                Debug.LogWarning("No audio clips available to assign.");
                return null;
            }
        }

        private void AssignSnoringAudio() {
            AudioClip snoringClip = Resources.Load<AudioClip>($"{relativeSnoringAudioPath}/snoring-8486");
            if (snoringClip != null) {
                EyeInteractable.snoringAudio = snoringClip;
            }
            else {
                Debug.LogWarning("snoring-8486 audio clip not found in the specified Resources folder.");
            }
        }
    }
}