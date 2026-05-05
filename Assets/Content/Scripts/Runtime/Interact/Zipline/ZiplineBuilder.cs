using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý việc thiết lập và tạo đường cáp Zipline (Đu dây) bằng thủ tục (Procedural).")]
    public class ZiplineBuilder : MonoBehaviour
    {
        [Tooltip("Mô hình giá đỡ hoặc cụm ròng rọc Zipline (Sẽ di chuyển dọc theo cáp).")]
        public GameObject ZiplineRack;

        [Tooltip("Trục hướng tới của hệ thống Zipline.")]
        public Axis ZiplineForward;

        [Tooltip("Trục hướng lên của hệ thống Zipline.")]
        public Axis ZiplineUpward;

        [Tooltip("Tọa độ điểm kết thúc (End Point) của đường cáp.")]
        public Vector3 ZiplineEnd;

        [Tooltip("Độ lệch tâm từ cáp đến vị trí treo của người chơi.")]
        public Vector3 CenterOffset;

        [Tooltip("Cấu hình cài đặt cho dây cáp (Độ chùng, độ trễ...).")]
        public ProceduralCable.CableSettings CableSettings;

        [Tooltip("Thành phần ProceduralCable đảm nhiệm việc vẽ dây cáp ra môi trường 3D.")]
        public ProceduralCable Cable;

        [Tooltip("Bật chế độ xem trước cáp trong Editor.")]
        public bool PreviewCable;

        [Tooltip("Bật chế độ xem trước mô hình người chơi (để căn chỉnh kích thước va chạm).")]
        public bool PreviewPlayer;

        [Tooltip("Bán kính giả lập người chơi.")]
        public float PlayerRadius = 0.3f;

        [Tooltip("Chiều cao giả lập người chơi.")]
        public float PlayerHeight = 1.8f;

        private void Reset()
        {
            ResetEndPosition();
        }

        public void ResetEndPosition()
        {
            ZiplineEnd = transform.position + new Vector3(1, 0, 0);
        }

        private void OnDrawGizmosSelected()
        {
            if (!PreviewCable || Cable == null || Cable.curvatorePoints == null)
                return;

            Gizmos.color = Color.red;
            for (int i = 1; i < Cable.curvatorePoints.Count; i++)
            {
                Vector3 start = transform.TransformPoint(Cable.curvatorePoints[i - 1]) + CenterOffset;
                Vector3 end = transform.TransformPoint(Cable.curvatorePoints[i]) + CenterOffset;
                Gizmos.DrawLine(start, end);

                if (i == 1) Gizmos.DrawWireSphere(start, 0.05f);
                Gizmos.DrawWireSphere(end, 0.05f);
            }

            if (PreviewPlayer)
            {
                Vector3 eval = Cable.EvalRaw(0.5f);
                Vector3 center = eval + CenterOffset;

                float height = (PlayerHeight - 0.6f) / 2f;
                Vector3 p1 = new Vector3(center.x, center.y - height, center.z);
                Vector3 p2 = new Vector3(center.x, center.y + height, center.z);

                Gizmos.color = Color.green;
                GizmosE.DrawWireCapsule(p1, p2, PlayerRadius);
            }
        }
    }
}