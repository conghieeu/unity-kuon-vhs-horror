using UnityEngine;
using Unity.Cinemachine;
using ThunderWire.Attributes;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [InspectorHeader("CCTV Camera")]
    public class CCTV_Camera : MonoBehaviour
    {
        public Camera LiveCamera;
        public CinemachineCamera VirtualCamera;

        [Header("Camera Joints")]
        public Transform VerticalJoint;
        public Transform HorizontalJoint;

        [Header("Joint Limits")]
        public MinMax VerticalLimits = new MinMax(-45, 45);
        public MinMax HorizontalLimits = new MinMax(-45, 45);

        [Header("Joint Axis")]
        public Axis VerticalAxis = Axis.X;
        public Axis HorizontalAxis = Axis.Z;

        [Header("Debug")]
        public bool VisualizeLimits;
        public Axis ForwardDirection = Axis.Y;
        public Axis VerticalUpward = Axis.X;
        public Axis HorizontalUpward = Axis.Z;

        private void OnDrawGizmos()
        {
            if (!VisualizeLimits)
                return;

            Vector3 forward = transform.Direction(ForwardDirection);

            if (VerticalJoint != null)
            {
                Vector3 vPos = VerticalJoint.transform.position;
                Vector3 vUpward = transform.Direction(VerticalUpward);
                HandlesDrawing.DrawLimitsArc(vPos, VerticalLimits, forward, vUpward, Color.red, radius: 0.35f);
            }

            if (HorizontalJoint != null)
            {
                Vector3 hPos = HorizontalJoint.transform.position;
                Vector3 hUpward = transform.Direction(HorizontalUpward);
                HandlesDrawing.DrawLimitsArc(hPos, HorizontalLimits, forward, hUpward, Color.cyan, radius: 0.35f);
            }
        }
    }
}