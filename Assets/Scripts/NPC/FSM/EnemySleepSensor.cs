using Assets.Scripts.GazeTrackingFeature;
using UnityEngine;

public class EnemySleepSensor : MonoBehaviour
{
    public bool isSleep;

    public bool IsSleeping()
    {
        EyeTrackingDebug.SnoringAudioPlaybackTrigger();
        return isSleep;
    }

}
