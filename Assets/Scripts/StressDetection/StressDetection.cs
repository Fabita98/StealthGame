using System;
using System.Linq;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

public class StressDetection : MonoBehaviour
{
    [NonSerialized] public float predictedStressValue;
    [SerializeField] private DataTracker DataTracker;
    private KalmanFilter kalmanFilter;
    private Matrix<float> z_preds;    // Predicted states (dz-dimensional)
    private Matrix<float> cov;        // Covariance matrix (dz-dimensional)

    private int dz = 1;  // State dimension (stress)
    // private int dx = 125 - 2;  // Observation dimension (eye, movement, controller, time)
    // private int dx = 87 - 2;  // Observation dimension (movement, controller, time)
    // private int dx = 78 - 2;  // Observation dimension (movement, controller)
    private int dx = 126;  // all

    private Matrix<float> transitionMatrix;
    private Matrix<float> observationMatrix;


    private static StressDetection _instance;
    public static StressDetection Instance => _instance;

    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        // Initialize the Kalman filter with a dynamic state and observation dimension
        kalmanFilter = new KalmanFilter(dz, dx);

        // Initialize transition and observation matrices
        transitionMatrix = Matrix<float>.Build.DenseIdentity(dz);      // dz x dz
        observationMatrix = Matrix<float>.Build.Dense(dx, dz, (i, j) => 1f); // dx x dz

        // Initialize arrays for predictions and covariance
        z_preds = Matrix<float>.Build.Dense(1, dz); // Placeholder for predictions, will resize as needed
        cov = Matrix<float>.Build.Dense(1, dz);     // Placeholder for covariance
    }

    void LateUpdate()
    {
        // Collect current frame data
        // float[] eyeMovement = GetEyeMovementData();
        // float[] headMovement = GetHeadMovementData();
        // float[] controllerPress = GetControllerPressData();
        // float heartRate = GetHeartRateData();

        // Combine inputs into a single observation vector
        // var observation = Vector<float>.Build.DenseOfArray(new float[]
        // {
        //     eyeMovement[0], eyeMovement[1],
        //     headMovement[0], headMovement[1], headMovement[2],
        //     controllerPress[0], controllerPress[1],
        //     heartRate
        // });
        
        Vector<float> observation = Vector<float>.Build.DenseOfArray(DataTracker.GetLatestDataRow().Skip(2).ToArray());

        // Apply the Kalman filter at each frame
        // float[] controlInput = GetControlInput(); // Control input (u_test)
        float[] controlInput = null;

        // Call function to apply the Kalman filter and get the updated state
        var predictedStress = ApplyOnlineInputKalman(dz, dx, observation, controlInput);
        
        // Output the predicted stress level
        predictedStressValue = predictedStress.Item1[0];
        Debug.LogWarning("Predicted Stress(" + Time.timeAsDouble + "): "  + predictedStressValue);
        // Debug.LogWarning("Predicted Stress(" + Time.timeAsDouble + "): "  + predictedStress.Item2);
    }

    private (Vector<float>, Matrix<float>) ApplyOnlineInputKalman(int dz, int dx, Vector<float> observation, float[] controlInput)
    {
        // Initialize the covariance and mean if it's the first frame
        if (Time.frameCount == 1)
        {
            z_preds = Matrix<float>.Build.Dense(1, dz); // Initial state (zeros)
            cov = Matrix<float>.Build.DenseIdentity(dz); // Initial covariance (identity matrix)
        }

        // Predict the next state using the Kalman filter
        Vector<float> nextState;
        Matrix<float> nextCovariance;

        if (Time.frameCount == 1) // First frame
        {
            (nextState, nextCovariance) = kalmanFilter.FilterUpdate(Vector<float>.Build.Dense(dz, 0), Matrix<float>.Build.DenseIdentity(dz),
                observation, transitionMatrix, observationMatrix 
                // , Vector<float>.Build.DenseOfArray(controlInput)
                );
        }
        else // Update based on previous state
        {
            (nextState, nextCovariance) = kalmanFilter.FilterUpdate(z_preds.Row(0), cov, observation, transitionMatrix, observationMatrix
                // , Vector<float>.Build.DenseOfArray(controlInput)
                );
        }

        // Update the predicted state and covariance
        z_preds.SetRow(0, nextState);  // Store the predicted state
        cov = nextCovariance;          // Update the covariance

        return (nextState, nextCovariance);  // Return both the updated state and covariance
    }


    private void NormalizeInputData()
    {
        
    }

    // Dummy methods for collecting inputs
    private float[] GetEyeMovementData() { return new float[] { Random.value, Random.value }; }
    private float[] GetHeadMovementData() { return new float[] { Random.value, Random.value, Random.value }; }
    private float[] GetControllerPressData() { return new float[] { Random.value, Random.value }; }
    private float GetHeartRateData() { return Random.value * 100; }
    private float[] GetControlInput() { return new float[] { 0.1f }; } // Dummy control input, adjust accordingly
}
