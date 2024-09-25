using MathNet.Numerics.LinearAlgebra;

public class KalmanFilter
{
    private Matrix<float> processCovariance;      // Q: Process noise covariance
    private Matrix<float> observationCovariance;  // R: Observation noise covariance

    public KalmanFilter(int dz, int dx)
    {
        // Initialize default covariance matrices
        processCovariance = Matrix<float>.Build.DenseIdentity(dz);      // Q: Process noise covariance
        observationCovariance = Matrix<float>.Build.DenseIdentity(dx);  // R: Observation noise covariance
    }

    // Method that returns both the updated state mean and covariance
    public (Vector<float>, Matrix<float>) FilterUpdate(Vector<float> previousStateMean, Matrix<float> previousStateCovariance,
                                                       Vector<float> observation, Matrix<float> transitionMatrix,
                                                       Matrix<float> observationMatrix, Vector<float> controlInput = null)
    {
        // Step 1: Predict the next state (prior)
        Vector<float> predictedStateMean = transitionMatrix * previousStateMean;

        if (controlInput != null)
        {
            predictedStateMean += controlInput; // Add control input to the prediction
        }

        Matrix<float> predictedStateCovariance = transitionMatrix * previousStateCovariance * transitionMatrix.Transpose() + processCovariance;

        // Step 2: Compute the Kalman Gain
        var S = observationMatrix * predictedStateCovariance * observationMatrix.Transpose() + observationCovariance;
        var K = predictedStateCovariance * observationMatrix.Transpose() * S.Inverse(); // Kalman Gain

        // Step 3: Update the state with the observation (correction step)
        Vector<float> y = observation - (observationMatrix * predictedStateMean); // Innovation (Residual)
        Vector<float> updatedStateMean = predictedStateMean + K * y;

        // Update covariance
        Matrix<float> updatedStateCovariance = (Matrix<float>.Build.DenseIdentity(K.RowCount) - K * observationMatrix) * predictedStateCovariance;

        // Return both the updated state mean and the updated covariance
        return (updatedStateMean, updatedStateCovariance);
    }
}
