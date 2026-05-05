using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Zipline Interact")]
    [Summary("Quản lý tương tác để bắt đầu đu dây Zipline. Gắn trên Collider điểm bắt đầu của Zipline.")]
    public class ZiplineInteract : MonoBehaviour, IStateInteract
    {
        [Tooltip("Tham chiếu đến hệ thống ZiplineBuilder cấu hình đường cáp này.")]
        public ZiplineBuilder ZiplineBuilder;

        public StateParams OnStateInteract()
        {
            Vector3 start = ZiplineBuilder.Cable._startTransform.position;
            Vector3 end = ZiplineBuilder.Cable._endTransform.position;
            Vector3 curvatore = ZiplineBuilder.Cable.CurvatorePoint;

            return new StateParams()
            {
                stateKey = PlayerStateMachine.ZIPLINE_STATE,
                stateData = new StorableCollection()
                {
                    { "object", gameObject },
                    { "start", start },
                    { "end", end },
                    { "curvatore", curvatore },
                    { "center", ZiplineBuilder.CenterOffset }
                }
            };
        }
    }
}