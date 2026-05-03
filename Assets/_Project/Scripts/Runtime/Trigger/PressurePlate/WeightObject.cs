using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class WeightObject : MonoBehaviour
    {
        public float ObjectWeight;
        public float StackedWeight;

        [Tooltip("Use the rigidbody mass instead of the custom object weight.")]
        public bool UseRigidbodyMass;
        [Tooltip("Allow mass stacking when an object is added on top of another object.")]
        public bool AllowStacking;

        private PressurePlateTrigger pressurePlate;
        private readonly List<WeightObject> weightsAbove = new();
        private WeightObject weightBelow;

        private Rigidbody _rigidbody;
        public Rigidbody Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody>();

                return _rigidbody;
            }
        }

        public float TotalMass
        {
            get
            {
                float weight = UseRigidbodyMass ? Rigidbody.mass : ObjectWeight;
                return weight + StackedWeight;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            WeightObject otherWeightObject = collision.gameObject.GetComponent<WeightObject>();
            PressurePlateTrigger plate = collision.gameObject.GetComponent<PressurePlateTrigger>();

            if (plate)
            {
                pressurePlate = plate;
                plate.OnWeightObjectStack(TotalMass);
            }
            else if (otherWeightObject && AllowStacking)
            {
                if (collision.contacts[0].normal.y < -0.5)
                {
                    if (!weightsAbove.Contains(otherWeightObject) && !otherWeightObject.weightBelow)
                    {
                        weightsAbove.Add(otherWeightObject);
                        otherWeightObject.weightBelow = this;

                        StackedWeight += otherWeightObject.TotalMass;
                        OnTotalMassChange(otherWeightObject.TotalMass);
                    }
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            WeightObject otherWeightObject = collision.gameObject.GetComponent<WeightObject>();
            PressurePlateTrigger plate = collision.gameObject.GetComponent<PressurePlateTrigger>();

            if (plate && pressurePlate == plate)
            {
                pressurePlate.OnWeightObjectStack(-TotalMass);
                pressurePlate = null;
            }

            if (otherWeightObject && AllowStacking)
            {
                if (weightsAbove.Contains(otherWeightObject))
                {
                    weightsAbove.Remove(otherWeightObject);
                    otherWeightObject.weightBelow = null;

                    StackedWeight -= otherWeightObject.TotalMass;
                    OnTotalMassChange(-otherWeightObject.TotalMass);
                }
            }
        }

        public void OnTotalMassChange(float massChange)
        {
            if (weightBelow)
            {
                weightBelow.OnTotalMassChange(massChange);
                weightBelow.StackedWeight += massChange;
            }
            else if (pressurePlate)
            {
                pressurePlate.OnWeightObjectStack(massChange);
            }
        }
    }
}
