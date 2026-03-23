using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class AutoRotate : MonoBehaviour
    {
        public Axis RotateAxis;
        public float RotateSpeed;

        private Vector3 axis;

        private void Awake()
        {
            axis = transform.Direction(RotateAxis);
        }

        private void Update()
        {
            transform.Rotate(RotateSpeed * Time.deltaTime * axis);
        }
    }
}