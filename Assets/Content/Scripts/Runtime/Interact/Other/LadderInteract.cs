using UnityEngine;
using UHFPS.Tools;
using UHFPS.Input;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý tương tác với Thang leo (Ladder), định nghĩa các điểm bắt đầu, điểm kết thúc và giới hạn góc nhìn.")]
    public class LadderInteract : MonoBehaviour, IStateInteract
    {
        [Tooltip("Mô hình chính của chiếc thang (Để tính chiều cao hoặc hiển thị).")]
        public GameObject LadderPart;

        [Tooltip("Khoảng cách tăng thêm theo trục dọc sau mỗi bước di chuyển.")]
        public float VerticalIncrement;

        [Tooltip("Vị trí điểm cao nhất của thang (Offset so với tâm).")]
        public Vector3 LadderUpOffset;

        [Tooltip("Vị trí điểm bước ra khỏi thang ở trên cùng (Offset).")]
        public Vector3 LadderExitOffset;

        [Tooltip("Điểm uốn cong đường dẫn khi người chơi bước ra (Arc Offset).")]
        public Vector3 LadderArcOffset;

        [Tooltip("Vị trí tâm của chân thang.")]
        public Vector3 CenterOffset;

        [Tooltip("Giới hạn góc xoay của chuột khi đang leo thang.")]
        public bool UseMouseLimits = true;

        [Tooltip("Giới hạn góc nhìn lên/xuống.")]
        public MinMax MouseVerticalLimits = new MinMax(-60, 90);

        [Tooltip("Giới hạn góc nhìn trái/phải.")]
        public MinMax MouseHorizontalLimits = new MinMax(-80, 80);

        [Tooltip("Bật hiển thị hình vẽ (Gizmos) trên Scene.")]
        public bool DrawGizmos = true;
        public bool DrawGizmosSteps = true;
        public bool DrawGizmosLabels = true;
        public bool DrawPlayerPreview = true;
        public bool DrawPlayerAtEnd = true;
        public float PlayerRadius = 0.3f;
        public float PlayerHeight = 1.8f;

        public Vector3 StartPos => transform.TransformPoint(CenterOffset);
        public Vector3 EndPos => transform.TransformPoint(LadderUpOffset + CenterOffset);
        public Vector3 ExitPos => transform.TransformPoint(LadderUpOffset + CenterOffset + LadderExitOffset);
        public Vector3 ArcPos => transform.TransformPoint(LadderUpOffset + CenterOffset + LadderArcOffset);

        public StateParams OnStateInteract()
        {
            return new StateParams()
            {
                stateKey = PlayerStateMachine.LADDER_STATE,
                stateData = new StorableCollection()
                {
                    { "transform", transform },
                    { "start", StartPos },
                    { "end", EndPos },
                    { "exit", ExitPos },
                    { "arc", ArcPos },
                    { "useLimits", UseMouseLimits },
                    { "verticalLimits", MouseVerticalLimits },
                    { "horizontalLimits", MouseHorizontalLimits },
                }
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (DrawGizmos)
            {
                Gizmos.color = Color.green.Alpha(0.5f);
                Gizmos.DrawSphere(StartPos, 0.1f);

                Gizmos.color = Color.yellow.Alpha(0.5f);
                Gizmos.DrawSphere(EndPos, 0.1f);

                Gizmos.color = Color.white.Alpha(0.5f);
                Gizmos.DrawLine(StartPos, EndPos);

                Gizmos.color = Color.red.Alpha(0.5f);
                Gizmos.DrawSphere(ExitPos, 0.1f);

                float radius = 0.75f;
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.green.Alpha(0.01f);
                UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, radius);
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, radius);
#endif
                Gizmos.color = Color.white;
                GizmosE.DrawGizmosArrow(transform.position, transform.forward * radius);

                if (DrawPlayerPreview)
                {
                    Vector3 center = ExitPos;
                    if (!DrawPlayerAtEnd) center = Vector3.Lerp(StartPos, EndPos, 0.5f);

                    float height = (PlayerHeight - 0.6f) / 2f;
                    Vector3 p1 = new Vector3(center.x, center.y - height, center.z);
                    Vector3 p2 = new Vector3(center.x, center.y + height, center.z);
                    Gizmos.color = Color.green;
                    GizmosE.DrawWireCapsule(p1, p2, PlayerRadius);
                }

                if (DrawGizmosLabels)
                {
                    GizmosE.DrawCenteredLabel(StartPos, "Start");
                    if (LadderUpOffset != Vector3.zero)
                        GizmosE.DrawCenteredLabel(EndPos, "End");
                    if (LadderExitOffset != Vector3.zero)
                        GizmosE.DrawCenteredLabel(ExitPos, "Exit");
                }

                if (DrawGizmosSteps)
                {
                    Gizmos.color = new Color(1f, 0.65f, 0f, 0.5f);
                    Gizmos.DrawSphere(ArcPos, 0.05f);

#if UNITY_EDITOR
                    if (DrawGizmosLabels && LadderArcOffset != Vector3.zero)
                        GizmosE.DrawCenteredLabel(ArcPos, "Arc Point");
#endif

                    Vector3 llp = VectorE.QuadraticBezier(EndPos, ExitPos, ArcPos, 0);
                    Gizmos.color = Color.white.Alpha(0.5f);

                    int steps = 20;
                    for (int i = 1; i <= steps; i++)
                    {
                        float t = i / (float)steps;
                        Vector3 lp = VectorE.QuadraticBezier(EndPos, ExitPos, ArcPos, t);
                        Gizmos.DrawLine(llp, lp);
                        llp = lp;
                    }
                }
            }
        }
    }
}