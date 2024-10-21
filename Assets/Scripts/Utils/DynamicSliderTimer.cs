using System;
using Assets.Scripts.GazeTrackingFeature;
using UnityEngine;
using UnityEngine.UI;  // Required for UI elements

public class DynamicSliderTimer : MonoBehaviour
{
    public Slider timerSlider;     // Reference to the UI Slider
    public float totalTime = 10f;  // Total time in seconds
    private float timeRemaining;   // Current remaining time
    private bool isTimerRunning = false;
    // private GameObject _enemyGameObject;

    private void Awake()
    {
        // _enemyGameObject = transform.parent.parent.gameObject;
        totalTime = EyeInteractable.snoringCooldownEndTime;
    }

    void OnEnable()
    {
        // Reset the timer
        timeRemaining = totalTime;
        timerSlider.maxValue = totalTime;
        timerSlider.value = totalTime;

        // Start the timer
        isTimerRunning = true;
    }

    void Update()
    {
        // If the timer is running, count down
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                // Decrease the time remaining and update the slider value
                timeRemaining -= Time.deltaTime;
                timerSlider.value = timeRemaining;
            }
            else
            {
                // Time's up, stop the timer and disable the object
                timeRemaining = 0;
                isTimerRunning = false;
                gameObject.SetActive(false);  // Disable the object
            }
        }
    }
}