using UnityEngine;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class ZiplineBuilder : MonoBehaviour
    {
        public GameObject ZiplineRack;
        public Axis ZiplineForward;
        public Axis ZiplineUpward;

        public Vector3 ZiplineEnd;
        public Vector3 CenterOffset;

        public ProceduralCable.CableSettings CableSettings;
        public ProceduralCable Cable;

        public bool PreviewCable;
        public bool PreviewPlayer;
        public float PlayerRadius = 0.3f;
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