using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Transform
{
    [Category("Extensions/Transform")]
    [Description("Liên tục xoay agent đến các góc ngẫu nhiên trong phạm vi được chỉ định.")]
    [Name("Random Rotate")]
    public class RandomRotate : ActionTask<UnityEngine.Transform>
    {
        public enum SpeedMode { Fixed, Random }

        [Name("Min Euler")]
        public BBParameter<Vector3> minEuler = new Vector3(0, 0, 0);
        [Name("Max Euler")]
        public BBParameter<Vector3> maxEuler = new Vector3(360, 360, 360);

        [Name("Speed Mode")]
        public SpeedMode speedMode = SpeedMode.Fixed;

        [ShowIf("speedMode", 0)] // Fixed
        [Name("Fixed Speed")]
        public BBParameter<float> fixedSpeed = 100f;

        [ShowIf("speedMode", 1)] // Random
        [Name("Min Speed")]
        public BBParameter<float> minSpeed = 50f;
        [ShowIf("speedMode", 1)] // Random
        [Name("Max Speed")]
        public BBParameter<float> maxSpeed = 150f;

        [Name("Local Space")]
        public bool isLocal = true;

        private Quaternion _targetRotation;
        private float _currentSpeed;

        protected override string info
        {
            get { return string.Format("Random Rotate ({0})", speedMode); }
        }

        protected override void OnExecute()
        {
            PickNewTarget();
        }

        protected override void OnUpdate()
        {
            if (agent == null)
            {
                EndAction(false);
                return;
            }

            Quaternion currentRotation = isLocal ? agent.localRotation : agent.rotation;
            
            // Rotate towards the target
            Quaternion nextRotation = Quaternion.RotateTowards(currentRotation, _targetRotation, _currentSpeed * Time.deltaTime);

            if (isLocal)
                agent.localRotation = nextRotation;
            else
                agent.rotation = nextRotation;

            // Check if we reached the target (small threshold to avoid floating point issues)
            if (Quaternion.Angle(nextRotation, _targetRotation) < 0.1f)
            {
                PickNewTarget();
            }
        }

        private void PickNewTarget()
        {
            float x = Random.Range(minEuler.value.x, maxEuler.value.x);
            float y = Random.Range(minEuler.value.y, maxEuler.value.y);
            float z = Random.Range(minEuler.value.z, maxEuler.value.z);
            
            _targetRotation = Quaternion.Euler(x, y, z);

            if (speedMode == SpeedMode.Fixed)
            {
                _currentSpeed = fixedSpeed.value;
            }
            else
            {
                _currentSpeed = Random.Range(minSpeed.value, maxSpeed.value);
            }
        }
    }
}