using Assets.Scripts.GazeTrackingFeature;
using UnityEngine;

public class CompleteGameActivationDetector : MonoBehaviour {
    private void OnEnable() => EyeTrackingDebug.CompleteGameEventTrigger();
}