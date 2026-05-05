using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("AI Waypoint")]
    [Summary("Đánh dấu một điểm (Waypoint) trên bản đồ để AI di chuyển tới trong lúc đi tuần (Patrol).")]
    public class AIWaypoint : MonoBehaviour
    {
        [Tooltip("NPC/Kẻ địch nào đang giữ điểm này (Ngăn các NPC khác cùng đi tới mục tiêu này).")]
        public GameObject ReservedBy;
    }
}