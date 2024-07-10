using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;

public class StressDetection : MonoBehaviour
{
    private KalmanFilter kalmanFilter;
    private const int frameWindow = 5;
    private const float normalHeartRateMin = 60f;
    private const float normalHeartRateMax = 100f;
    private const float normalizerMin = 0f;
    private const float normalizerMax = 1f;

    private List<float[]> accumulatedEyeMovementData = new List<float[]>();
    private List<float[]> accumulatedHeadMovementData = new List<float[]>();
    private List<float[]> accumulatedControllerPressData = new List<float[]>();
    private List<float> accumulatedHeartRateData = new List<float>();

    private int currentFrame = 0;
    private float currentStressValue = 0f;

    void Start()
    {
        // Initialize Kalman Filter
        int dz = 1; // Assuming we estimate a single stress level
        int dx = 3 + 1; // Example: 3 features from other sensors + 1 heart rate feature
        kalmanFilter = new KalmanFilter(dz, dx);
    }

    void Update()
    {
        // Collect current frame data
        float[] eyeMovement = GetEyeMovementData();
        float[] headMovement = GetHeadMovementData();
        float[] controllerPress = GetControllerPressData();
        float heartRate = GetHeartRateData();

        accumulatedEyeMovementData.Add(eyeMovement);
        accumulatedHeadMovementData.Add(headMovement);
        accumulatedControllerPressData.Add(controllerPress);
        accumulatedHeartRateData.Add(NormalizeHeartRate(heartRate));

        currentFrame++;

        if (currentFrame >= frameWindow)
        {
            // Combine input data
            List<float[]> inputData = CombineInputData(accumulatedEyeMovementData, accumulatedHeadMovementData, accumulatedControllerPressData, accumulatedHeartRateData);

            // Apply Kalman Filter
            var zPreds = ApplyKalmanFilter(inputData, 1, inputData[0].Length);

            // Normalize final predictions
            var normalizedZPreds = NormalizePredictions(zPreds, normalizerMin, normalizerMax);

            // Update current stress value
            currentStressValue = normalizedZPreds.Last()[0]; // Assuming a single stress value

            // Clear accumulated data for the next window
            accumulatedEyeMovementData.Clear();
            accumulatedHeadMovementData.Clear();
            accumulatedControllerPressData.Clear();
            accumulatedHeartRateData.Clear();
            currentFrame = 0;

            Debug.Log($"Stress Value: {currentStressValue}");
        }
    }

    float[] GetEyeMovementData()
    {
        // Replace with actual eye movement data retrieval logic
        return new float[] { Random.Range(0f, 1f) };
    }

    float[] GetHeadMovementData()
    {
        // Replace with actual head movement data retrieval logic
        return new float[] { Random.Range(0f, 1f) };
    }

    float[] GetControllerPressData()
    {
        // Replace with actual controller press data retrieval logic
        return new float[] { Random.Range(0f, 1f) };
    }

    float GetHeartRateData()
    {
        // Replace with actual heart rate data retrieval logic
        return Random.Range(60f, 100f);
    }

    float NormalizeHeartRate(float heartRate)
    {
        // Normalize the heart rate value to the range [0, 1]
        float normalized = (heartRate - normalHeartRateMin) / (normalHeartRateMax - normalHeartRateMin);
        return Mathf.Clamp(normalized, normalizerMin, normalizerMax);
    }

    List<float[]> CombineInputData(List<float[]> eyeMovementData, List<float[]> headMovementData, List<float[]> controllerPressData, List<float> heartRateData)
    {
        List<float[]> combinedData = new List<float[]>();
        for (int i = 0; i < eyeMovementData.Count; i++)
        {
            List<float> combinedFrameData = new List<float>();
            combinedFrameData.AddRange(eyeMovementData[i]);
            combinedFrameData.AddRange(headMovementData[i]);
            combinedFrameData.AddRange(controllerPressData[i]);
            combinedFrameData.Add(heartRateData[i]);
            combinedData.Add(combinedFrameData.ToArray());
        }
        return combinedData;
    }

    List<Vector<float>> ApplyKalmanFilter(List<float[]> inputData, int dz, int dx)
    {
        List<Vector<float>> zPreds = new List<Vector<float>>();
        Vector<float> prevStateMean = Vector<float>.Build.Dense(dz);
        Matrix<float> prevStateCovariance = Matrix<float>.Build.DenseIdentity(dz);

        for (int i = 0; i < inputData.Count; i++)
        {
            Vector<float> observation = Vector<float>.Build.DenseOfArray(inputData[i]);
            (prevStateMean, prevStateCovariance) = kalmanFilter.FilterUpdate(prevStateMean, prevStateCovariance, observation);
            zPreds.Add(prevStateMean);
        }

        return zPreds;
    }

    List<float[]> NormalizePredictions(List<Vector<float>> predictions, float min, float max)
    {
        List<float[]> normalizedPredictions = new List<float[]>();

        foreach (var prediction in predictions)
        {
            float[] normalized = new float[prediction.Count];
            for (int i = 0; i < prediction.Count; i++)
            {
                // Normalize the prediction values
                normalized[i] = (prediction[i] - min) / (max - min);
                // Clamp between min and max
                normalized[i] = Mathf.Clamp(normalized[i], min, max);
            }
            normalizedPredictions.Add(normalized);
        }

        return normalizedPredictions;
    }
}
