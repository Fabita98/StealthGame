using UnityEngine;

public class GazeVoiceControl : MonoBehaviour
{
    [Header("Gaze Direction")]
    Vector3 targetDirection;
    Vector3 gazeDirection;
    [SerializeField] OVREyeGaze eyeGaze;

    [Header("Angle and Time")]
    readonly float angleThreshold = 10f;
    float gazeTime = 0f;
    readonly float requiredGazeTime = 3f;

    private LineRenderer gazeLineRenderer;

    private void Start()
    {
        // Check if eyeGaze is assigned in the Inspector
        if (eyeGaze == null)
        {
            Debug.LogError("eyeGaze is not assigned in the Inspector.");
            return;
        }

        // Check if the eye tracking feature is enabled
        if (eyeGaze.EyeTrackingEnabled.Equals(false))
        {
            Debug.LogError("Eye tracking is not enabled or not supported on this device.");
            return;
        }
        else { 
            //eyeGaze.ReferenceFrame = Camera.current.transform;
            Debug.Log("Eyes retrieved!");
        }
        

        // Check if the Camera.current is available
        if (Camera.current == null)
        {
            Debug.LogError("Camera.current is null. " +
                "Make sure the main camera is tagged as 'MainCamera'" +
                " or set the ReferenceFrame manually to the correct camera.");
            return;
        }

        InitializeLineRenderer();
    }

    void Update()
    {
        if (eyeGaze && eyeGaze.EyeTrackingEnabled)
        {
            // Update gaze direction from eyeGaze's reference frame
            gazeDirection = eyeGaze.ReferenceFrame.forward;

            // Update the LineRenderer to visualize the gaze direction
            UpdateLineRenderer(eyeGaze.ReferenceFrame.position, eyeGaze.ReferenceFrame.position + gazeDirection * 10);

            Debug.Log("Eyes are working in update! ");
        }

        // Your existing code for handling gaze direction and actions...
    }

    private void InitializeLineRenderer()
    {
        GameObject gazeLineObject = new GameObject("GazeLine");
        gazeLineRenderer = gazeLineObject.AddComponent<LineRenderer>();

        // Configure the LineRenderer
        gazeLineRenderer.startWidth = 0.02f;
        gazeLineRenderer.endWidth = 0.02f;
        gazeLineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        gazeLineRenderer.material.color = Color.red;
        gazeLineRenderer.positionCount = 2;
    }

    private void UpdateLineRenderer(Vector3 start, Vector3 end)
    {
        gazeLineRenderer.SetPosition(0, start);
        gazeLineRenderer.SetPosition(1, end);
    }
}