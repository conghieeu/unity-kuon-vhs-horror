using UnityEngine;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Procedural animation of damped harmonic oscillator.
    /// </summary>
    public sealed class Spring3D
    {
        public Vector3 currentValue;
        public Vector3 currentVelocity;
        public Vector3 currentAcceleration;
        public Vector3 targetValue;
        public SpringSettings springSettings;

        private const float stepSizeConstant = 0.01f;

        public bool IsIdle { get; private set; } = true;

        public Spring3D() : this(SpringSettings.Default) { }

        public Spring3D(SpringSettings springSettings)
        {
            this.springSettings = springSettings;
            Reset();
        }

        public void SetTarget(Vector3 targetVector)
        {
            targetValue = targetVector;
            IsIdle = false;
        }

        public void Reset()
        {
            currentValue = Vector3.zero;
            currentVelocity = Vector3.zero;
            currentAcceleration = Vector3.zero;
            targetValue = Vector3.zero;
            IsIdle = true;
        }

        public Vector3 Evaluate(float deltaTime)
        {
            if (IsIdle) return Vector3.zero;

            float dampingFactor = springSettings.Damping;
            float stiffnessFactor = springSettings.Stiffness;
            float objectMass = springSettings.Mass;

            Vector3 currentVal = currentValue;
            Vector3 currentVel = currentVelocity;
            Vector3 currentAcc = currentAcceleration;

            // actual step size based on the time delta and speed.
            float actualStepSize = deltaTime * springSettings.Speed;

            // cap the effective step size at the constant or slightly less than the actual step size.
            float effectiveStepSize = Mathf.Min(stepSizeConstant, actualStepSize - 0.001f);

            // determine the number of simulation steps.
            float calculationSteps = (int)(actualStepSize / effectiveStepSize + 0.5f);

            for (var i = 0; i < calculationSteps; i++)
            {
                // adjust the last time step to ensure the total steps add up to actualStepSize.
                var dt = Mathf.Abs(i - (calculationSteps - 1)) < 0.01f ? actualStepSize - i * effectiveStepSize : effectiveStepSize;

                // update position based on current velocity, acceleration, and time step.
                currentVal += currentVel * dt + currentAcc * (dt * dt * 0.5f);

                // calculate new acceleration based on Hooke's law (spring force) and Newton's second law (F = ma).
                Vector3 newAcc = (-stiffnessFactor * (currentVal - targetValue) + (-dampingFactor * currentVel)) / objectMass;

                // update velocity based on the average of current and new accelerations and the time step.
                currentVel += (currentAcc + newAcc) * (dt * 0.5f);

                // update acceleration to the new value.
                currentAcc = newAcc;
            }

            currentValue = currentVal;
            currentVelocity = currentVel;
            currentAcceleration = currentAcc;

            // check if the object has stopped moving.
            if (Mathf.Approximately(currentAcc.sqrMagnitude, 0f))
                IsIdle = true;

            return currentValue;
        }
    }
}
