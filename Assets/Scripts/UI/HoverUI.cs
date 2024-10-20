using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GazeTrackingFeature
{
    public class HoverUI : MonoBehaviour
    {
        [SerializeField] private Text text;

        private void Update() => text.text = $"Hover time: {EyeInteractable.HoveringTime}\n A_Key hold time: {EyeTrackingDebug.buttonHoldTime}";
    } 
}