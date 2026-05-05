using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [ExecuteAlways, InspectorHeader("Procedural Cable Point")]
    [Summary("Điểm mốc của dây cáp Procedural. Khi điểm này di chuyển, dây cáp sẽ tự động tạo lại.")]
    public class ProceduralCablePoint : MonoBehaviour
    {
        [ReadOnly]
        [Tooltip("Tham chiếu đến hệ thống dây cáp Procedural.")]
        public ProceduralCable proceduralCable;

        private void Update()
        {
            if (proceduralCable && !proceduralCable.manualGeneration && transform.hasChanged)
            {
                transform.hasChanged = false;
                proceduralCable.RegenerateCable();
            }
        }
    }
}