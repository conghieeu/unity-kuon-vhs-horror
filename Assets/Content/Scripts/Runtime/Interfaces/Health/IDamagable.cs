using UnityEngine;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Giao diện cho các thực thể có thể nhận sát thương.
    /// </summary>
    public interface IDamagable
    {
        /// <summary>
        /// Định nghĩa hành vi khi thực thể nhận sát thương.
        /// </summary>
        void OnApplyDamage(int damage, Transform sender = null);

        /// <summary>
        /// Định nghĩa hành vi khi thực thể nhận sát thương tối đa (ngay lập tức chết/hỏng).
        /// </summary>
        void ApplyDamageMax(Transform sender = null);
    }
}