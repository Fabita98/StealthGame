using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GazeTrackingFeature
{
    public class SpeakingUI : MonoBehaviour
    {
        public Text speakingUIText;

        private void Awake()
        {
            speakingUIText.enabled = false;
        }

        private void OnEnable()
        {
            EyeTrackingDebug.OnRecordingAboutToStart += EnableSpeakingText;
            EyeTrackingDebug.OnRecordingStopped += DisableSpeakingText;
        }

        private void OnDisable()
        {
            EyeTrackingDebug.OnRecordingAboutToStart -= EnableSpeakingText;
            EyeTrackingDebug.OnRecordingStopped -= DisableSpeakingText;
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