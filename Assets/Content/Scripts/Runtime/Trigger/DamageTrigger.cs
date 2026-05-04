using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class DamageTrigger : MonoBehaviour, ISaveable
    {
        [Flags]
        public enum DamageReceiverEnum { Player = 1, Enemy = 2, Breakable = 4 }
        public enum DamageTypeEnum { Once, MoreTimes, Stay }

        public DamageReceiverEnum DamageReceiver = DamageReceiverEnum.Player;
        public DamageTypeEnum DamageType = DamageTypeEnum.Once;

        public Tag EnemyTag;
        public bool DamageInRange;
        public bool InstantDeath;

        public uint Damage;
        public MinMaxInt DamageRange;
        public float DamageRate;

        public UnityEvent<uint> OnDamage;

        private float damageTime;
        private bool damageOnce;

        private readonly List<Collider> damageables = new();

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IDamagable damageable) && !damageables.Contains(other))
            {
                SendDamage(other.gameObject, damageable);
                damageables.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (DamageType == DamageTypeEnum.MoreTimes)
            {
                if (other.TryGetComponent(out IDamagable _))
                    damageables.Remove(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (DamageType == DamageTypeEnum.Stay && !damageOnce && damageTime <= 0)
            {
                if (other.TryGetComponent(out IDamagable damageable))
                    SendDamage(other.gameObject, damageable);

                damageTime = DamageRate;
            }
        }

        private void SendDamage(GameObject obj, IDamagable damageable)
        {
            uint damage = DamageInRange ? (uint)DamageRange.Random() : Damage;
            if (InstantDeath) damageOnce = true;

            if (DamageReceiver.HasFlag(DamageReceiverEnum.Player) && obj.CompareTag("Player") && damageable is BaseHealthEntity player)
            {
                if (InstantDeath) player.ApplyDamageMax(transform);
                else player.OnApplyDamage((int)damage, transform);
            }
            else if (DamageReceiver.HasFlag(DamageReceiverEnum.Enemy) && obj.CompareTag(EnemyTag) && damageable is BaseHealthEntity enemy)
            {
                if (InstantDeath) enemy.ApplyDamageMax(transform);
                else enemy.OnApplyDamage((int)damage, transform);
            }
            else if (DamageReceiver.HasFlag(DamageReceiverEnum.Breakable) && damageable is BaseBreakableEntity breakable)
            {
                if (InstantDeath) breakable.ApplyDamageMax(transform);
                else breakable.OnApplyDamage((int)damage, transform);
            }

            OnDamage?.Invoke(damage);
        }

        private void Update()
        {
            if (DamageType == DamageTypeEnum.Stay && !damageOnce && damageTime > 0)
                damageTime -= Time.deltaTime;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(damageOnce), damageOnce },
            };
        }

        public void OnLoad(JToken data)
        {
            damageOnce = (bool)data[nameof(damageOnce)];
        }
    }
}