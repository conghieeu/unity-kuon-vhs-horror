using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Represents the base class for breakable entity health functions.
    /// </summary>
    public abstract class BaseBreakableEntity : SaveableBehaviour, IBreakableEntity
    {
        private int Health;
        public int EntityHealth
        {
            get { return Health; }
            set
            {
                OnHealthChanged(Health, value);
                Health = value;

                if (Health <= 0 && !isBroken)
                {
                    OnBreak();
                    isBroken = true;
                }
                else if (Health > 0 && isBroken)
                {
                    isBroken = false;
                }
            }
        }

        protected bool isBroken = false;

        public void InitializeHealth(int health)
        {
            Health = health;
            isBroken = false;
        }

        public virtual void OnApplyDamage(int damage, Transform sender = null)
        {
            if (isBroken) return;
            EntityHealth = Math.Clamp(EntityHealth - damage, 0, int.MaxValue);
        }

        public virtual void ApplyDamageMax(Transform sender = null)
        {
            if (isBroken) return;
            EntityHealth = 0;
        }

        /// <summary>
        /// Override this method to define custom behavior when the breakable entity is broken.
        /// </summary>
        public virtual void OnBreak() { }

        /// <summary>
        /// Override this method to define custom behavior when the entity health is changed.
        /// </summary>
        public virtual void OnHealthChanged(int oldHealth, int newHealth) { }
    }
}