using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GazeTrackingFeature
{
    public class HoverUI : MonoBehaviour
    {
        [SerializeField] private Text text;

        private void Update()
        {
            text.text = $"Hover\ntime: {EyeInteractable.HoveringTime}";
        }
    } 
}