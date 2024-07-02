using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GazeTrackingFeature
{
    public class SpeakingUI : MonoBehaviour
    {
        [SerializeField] private Text speakingUIText;

        private void Update()
        {
            if (EyeInteractable.readyToTalk) speakingUIText.enabled = true;
            else speakingUIText.enabled = false;
        }
    }
}