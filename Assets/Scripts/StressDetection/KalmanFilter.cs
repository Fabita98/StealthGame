using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class KalmanFilter
{
    private int stateDimension;
    private int observationDimension;

    private Matrix<float> transitionMatrix;
    private Matrix<float> observationMatrix;
    private Matrix<float> transitionCovariance;
    private Matrix<float> observationCovariance;

    public KalmanFilter(int dz, int dx, Matrix<float> transitionMatrix = null, Matrix<float> observationMatrix = null)
    {
        stateDimension = dz;
        observationDimension = dx;

        this.transitionMatrix = transitionMatrix ?? Matrix<float>.Build.Dense(dz, dz, 1.0f);
        this.observationMatrix = observationMatrix ?? Matrix<float>.Build.Dense(dx, dz, 1.0f);
        transitionCovariance = Matrix<float>.Build.DenseDiagonal(dz, 1.0f);
        observationCovariance = Matrix<float>.Build.DenseDiagonal(dx, 1.0f);
    }

    public (Vector<float>, Matrix<float>) FilterUpdate(Vector<float> prevStateMean, Matrix<float> prevStateCovariance, Vector<float> observation, Vector<float> transitionOffset = null)
    {
        // Prediction step
        Vector<float> predictedStateMean = transitionMatrix * prevStateMean;
        if (transitionOffset != null)
        {
            predictedStateMean += transitionOffset;
        }

        Matrix<float> predictedStateCovariance = transitionMatrix * prevStateCovariance * transitionMatrix.Transpose() + transitionCovariance;

        // Update step
        Vector<float> innovation = observation - (observationMatrix * predictedStateMean);
        Matrix<float> innovationCovariance = observationMatrix * predictedStateCovariance * observationMatrix.Transpose() + observationCovariance;

        Matrix<float> kalmanGain = predictedStateCovariance * observationMatrix.Transpose() * innovationCovariance.Inverse();
        Vector<float> updatedStateMean = predictedStateMean + (kalmanGain * innovation);
        Matrix<float> updatedStateCovariance = (Matrix<float>.Build.DenseIdentity(stateDimension) - (kalmanGain * observationMatrix)) * predictedStateCovariance;

        return (updatedStateMean, updatedStateCovariance);
    }
}
