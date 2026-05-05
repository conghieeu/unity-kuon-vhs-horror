using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("NPC Body Part")]
    [Summary("Quản lý một bộ phận cơ thể của NPC (Ví dụ: Đầu, Thân, Tay). Chuyển tiếp sát thương nhận được tới NPCHealth.")]
    public class NPCBodyPart : MonoBehaviour, IDamagable
    {
        [HideInInspector]
        public NPCHealth HealthScript;

        [Tooltip("Đánh dấu đây là phần Đầu (Headshot) để nhân sát thương.")]
        public bool IsHeadDamage;

        public void OnApplyDamage(int damage, Transform sender = null)
        {
            if (HealthScript == null)
                return;

            if (HealthScript.AllowHeadhsot && IsHeadDamage)
                damage = Mathf.RoundToInt(damage * HealthScript.HeadshotMultiplier);

            HealthScript.OnApplyDamage(damage, sender);
        }

        public void ApplyDamageMax(Transform sender = null)
        {
            if (HealthScript == null)
                return;

            HealthScript.ApplyDamageMax(sender);
        }
    }
}