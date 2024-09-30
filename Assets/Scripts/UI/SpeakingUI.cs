using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GazeTrackingFeature {
    public class SpeakingUI : MonoBehaviour
    {
        public Text speakingUIText;

        private void Awake()
        {
            speakingUIText.enabled = false;
        }

        private void OnEnable()
        {
            EyeTrackingDebug.OnPlaybackAboutToStart += EnableSpeakingText;
            EyeTrackingDebug.OnPlaybackStopped += DisableSpeakingText;
        }

        private void OnDisable()
        {
            EyeTrackingDebug.OnPlaybackAboutToStart -= EnableSpeakingText;
            EyeTrackingDebug.OnPlaybackStopped -= DisableSpeakingText;
        }

        private void EnableSpeakingText()
        {
            speakingUIText.enabled = true;
        }

        private void DisableSpeakingText()
        {
            speakingUIText.enabled = false;
        }
    }
}