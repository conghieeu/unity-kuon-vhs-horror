using UnityEngine;
using Unity.Cinemachine;
using ThunderWire.Attributes;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [InspectorHeader("CCTV Camera")]
    [Summary("Quản lý cấu hình trục xoay (Vertical/Horizontal) và giới hạn góc xoay của một camera an ninh (CCTV).")]
    public class CCTV_Camera : MonoBehaviour
    {
        [Tooltip("Camera dùng để xuất hình thực tế ra màn hình (RenderTexture).")]
        public Camera LiveCamera;

        [Tooltip("Camera ảo Cinemachine dùng khi người chơi trực tiếp xem qua góc nhìn Camera này.")]
        public CinemachineCamera VirtualCamera;

        [Header("Camera Joints")]
        [Tooltip("Khớp xoay lên/xuống.")]
        public Transform VerticalJoint;

        [Tooltip("Khớp xoay trái/phải.")]
        public Transform HorizontalJoint;

        [Header("Joint Limits")]
        [Tooltip("Giới hạn góc xoay lên/xuống.")]
        public MinMax VerticalLimits = new MinMax(-45, 45);

        [Tooltip("Giới hạn góc xoay trái/phải.")]
        public MinMax HorizontalLimits = new MinMax(-45, 45);

        [Header("Joint Axis")]
        [Tooltip("Trục không gian xoay lên/xuống.")]
        public Axis VerticalAxis = Axis.X;

        [Tooltip("Trục không gian xoay trái/phải.")]
        public Axis HorizontalAxis = Axis.Z;

        [Header("Debug")]
        [Tooltip("Hiển thị giới hạn góc xoay trong Editor.")]
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