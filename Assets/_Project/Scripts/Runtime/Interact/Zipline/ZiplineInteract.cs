using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Zipline Interact")]
    public class ZiplineInteract : MonoBehaviour, IStateInteract
    {
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