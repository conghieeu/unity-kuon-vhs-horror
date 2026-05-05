using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Thành phần gắn vào vật thể để định nghĩa trọng lượng, dùng để tương tác với PressurePlateTrigger.")]
    public class WeightObject : MonoBehaviour
    {
        [Tooltip("Trọng lượng của vật thể (nếu không dùng Rigidbody Mass).")]
        public float ObjectWeight;
        [Tooltip("Tổng trọng lượng của các vật thể khác đang đè lên trên vật thể này.")]
        public float StackedWeight;

        [Tooltip("Sử dụng khối lượng (Mass) của Rigidbody thay vì ObjectWeight tùy chỉnh.")]
        public bool UseRigidbodyMass;
        [Tooltip("Cho phép cộng dồn trọng lượng khi có một vật thể khác đè lên trên vật thể này.")]
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
