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
        // return isSleep;
        if (eyeInteractable.isSleeping) 
            Debug.Log("Enemy is sleeping");
        return eyeInteractable.isSleeping;
    }

}
