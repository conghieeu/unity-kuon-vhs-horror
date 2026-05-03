using UnityEngine;

namespace UHFPS.Runtime
{
    public interface IDamagable
    {
        /// <summary>
        /// Override this method to define custom behavior when an entity receives a damage call.
        /// </summary>
        void OnApplyDamage(int damage, Transform sender = null);

        /// <summary>
        /// Override this method to define custom behavior when applying maximum damage.
        /// </summary>
        void ApplyDamageMax(Transform sender = null);
    }
}