using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Lớp cơ sở định nghĩa các chức năng phá vỡ (Breakable) cho một thực thể.
    /// </summary>
    [ThunderWire.Attributes.Summary("Lớp cơ sở định nghĩa các chức năng phá vỡ (Breakable) cho một thực thể (Ví dụ: Thùng gỗ, Cửa kính).")]
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
        /// Ghi đè phương thức này để tự định nghĩa hành động khi thực thể bị vỡ.
        /// </summary>
        public virtual void OnBreak() { }

        /// <summary>
        /// Ghi đè phương thức này để tự định nghĩa hành động khi lượng máu của thực thể thay đổi.
        /// </summary>
        public virtual void OnHealthChanged(int oldHealth, int newHealth) { }
    }
}