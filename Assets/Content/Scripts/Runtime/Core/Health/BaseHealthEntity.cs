using System;
using UnityEngine;

namespace UHFPS.Runtime
{

    [ThunderWire.Attributes.Summary("Lớp cơ sở (Base Class) định nghĩa hệ thống máu (Health) cho mọi thực thể có thể bị sát thương (Người chơi, Quái vật, Đồ vật...).")]
    public abstract class BaseHealthEntity : MonoBehaviour, IHealthEntity
    {
        private int Health;
        public int EntityHealth
        {
            get => Health;
            set 
            {
                OnHealthChanged(Health, value);
                Health = value;

                if (Health <= 0 && !IsDead)
                {
                    OnHealthZero();
                    IsDead = true;
                }
                else if (Health > 0 && IsDead)
                {
                    IsDead = false;
                }

                if (Health >= MaxEntityHealth) OnHealthMax();
            }
        }

        public int MaxEntityHealth { get; set; }
        public bool IsDead = false;

        public void InitializeHealth(int health, int maxHealth = 100)
        {
            Health = health;
            MaxEntityHealth = maxHealth;
            OnHealthChanged(0, health);
            IsDead = false;
        }

        public virtual void OnApplyDamage(int damage, Transform sender = null)
        {
            if (IsDead) return;
            EntityHealth = Math.Clamp(EntityHealth - damage, 0, MaxEntityHealth);
        }

        public virtual void ApplyDamageMax(Transform sender = null)
        {
            if (IsDead) return;
            EntityHealth = 0;
        }

        public virtual void OnApplyHeal(int healAmount)
        {
            if (IsDead) return;
            EntityHealth = Math.Clamp(EntityHealth + healAmount, 0, MaxEntityHealth);
        }

        public virtual void ApplyHealMax()
        {
            if (IsDead) return;
            EntityHealth = MaxEntityHealth;
        }

        /// <summary>
        /// Ghi đè phương thức này để định nghĩa hành động khi lượng máu bị thay đổi.
        /// </summary>
        public virtual void OnHealthChanged(int oldHealth, int newHealth) { }

        /// <summary>
        /// Ghi đè phương thức này để định nghĩa hành động khi thực thể hết máu (chết).
        /// </summary>
        public virtual void OnHealthZero() { }

        /// <summary>
        /// Ghi đè phương thức này để định nghĩa hành động khi thực thể đạt lượng máu tối đa.
        /// </summary>
        public virtual void OnHealthMax() { }
    }
}