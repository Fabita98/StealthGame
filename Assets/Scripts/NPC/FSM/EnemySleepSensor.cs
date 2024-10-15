using Assets.Scripts.GazeTrackingFeature;
using UnityEngine;

public class EnemySleepSensor : MonoBehaviour
{
    public bool isSleep;
    EyeInteractable eyeInteractable;

    private void Awake() {
        eyeInteractable = GetComponent<EyeInteractable>();
    }

    public bool IsSleeping()
    {
        return eyeInteractable.readyToTalk;
    }

}
