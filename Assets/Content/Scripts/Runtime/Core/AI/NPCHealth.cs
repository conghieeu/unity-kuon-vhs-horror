using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý máu (Health) và trạng thái sống/chết (Chuyển sang dạng Ragdoll/Corpse) của NPC.")]
    public class NPCHealth : BaseHealthEntity, ISaveable
    {
        [System.Serializable]
        public struct BodySegment
        {
            public Rigidbody Rigidbody;
            public Collider Collider;
            public NPCBodyPart BodyPart;

            public BodySegment(Rigidbody rigidbody, Collider collider, NPCBodyPart bodyPart)
            {
               Rigidbody = rigidbody;
               Collider = collider;
                BodyPart = bodyPart;
            }
        }

        [Tooltip("Danh sách các bộ phận cơ thể (Ragdoll) của NPC để bật/tắt hiệu ứng vật lý khi chết.")]
        public List<BodySegment> BodySegments = new();

        [Tooltip("Danh sách các Component (như AI, Collider) sẽ bị tắt đi khi NPC chết.")]
        public List<Component> DisableComponents;

        [Tooltip("Xương hông (Hips) của NPC.")]
        public Transform Hips;

        [Tooltip("Collider phần đầu.")]
        public Collider Head;

        [Tooltip("Layer dành cho các phần cơ thể.")]
        public Layer BodyPartLayer;

        [Tooltip("Máu tối đa của NPC.")]
        public uint MaxHealth = 100;

        [Tooltip("Máu khởi điểm của NPC.")]
        public uint StartHealth = 100;

        [Tooltip("Hệ số nhân sát thương khi bị bắn trúng đầu.")]
        public float HeadshotMultiplier = 2f;

        [Tooltip("Cho phép nhân sát thương khi bắn trúng đầu không?")]
        public bool AllowHeadhsot = true;

        [Tooltip("Có xóa/ẩn xác (Corpse) của NPC sau một khoảng thời gian chết không?")]
        public bool RemoveCorpse;

        [Tooltip("Tắt object xác thay vì xóa hoàn toàn.")]
        public bool DisableCorpse;

        [Tooltip("Thời gian tồn tại của xác (giây) trước khi bị xóa/ẩn.")]
        public float CorpseRemoveTime = 10f;

        [Tooltip("Danh sách âm thanh phát ra khi nhận sát thương.")]
        public AudioClip[] DamageSounds;

        [Range(0f, 1f)] 
        public float DamageVolume = 1f;

        [Tooltip("Âm thanh phát ra khi NPC chết.")]
        public SoundClip DeathSound;

        [Tooltip("Sự kiện gọi ra khi NPC nhận sát thương, truyền vào lượng sát thương.")]
        public UnityEvent<int> OnTakeDamage;

        [Tooltip("Sự kiện gọi ra khi NPC chết.")]
        public UnityEvent OnDeath;

        [Tooltip("Sự kiện gọi ra khi xác của NPC bị xóa/ẩn.")]
        public UnityEvent OnCorpseRemove;

        private int lastDamageSound;
        private float corpseTime;
        private bool corpseRemoved;

        private void Awake()
        {
            if (!SaveGameManager.GameWillLoad)
                InitializeHealth((int)StartHealth, (int)MaxHealth);
        }

        private void Update()
        {
            if (!IsDead || corpseRemoved) 
                return;

            if(corpseTime > 0) corpseTime -= Time.deltaTime;
            else
            {
                if (DisableCorpse) gameObject.SetActive(false);
                OnCorpseRemove?.Invoke();
                corpseTime = 0;
                corpseRemoved = true;
            }
        }

        public override void OnApplyDamage(int damage, Transform sender = null)
        {
            if (IsDead || corpseRemoved)
                return;

            base.OnApplyDamage(damage, sender);
            OnTakeDamage?.Invoke(damage);

            if(DamageSounds.Length > 0)
            {
                int damageSound = GameTools.RandomUnique(0, DamageSounds.Length, lastDamageSound);
                GameTools.PlayOneShot3D(transform.position, DamageSounds[damageSound], DamageVolume, "ZombieDamageAudio");
                lastDamageSound = damageSound;
            }
        }

        public override void OnHealthZero()
        {
            EnableRagdoll(true);
            OnDeath?.Invoke();
            corpseTime = CorpseRemoveTime;

            foreach (var component in DisableComponents)
            {
                if(component is Behaviour behaviour)
                    behaviour.enabled = false;
                else if(component is Collider collider)
                    collider.enabled = false;
            }

            GameTools.PlayOneShot3D(transform.position, DeathSound, "ZombieDeathAudio");
        }

        private void EnableRagdoll(bool enabled)
        {
            foreach (BodySegment bodyPart in BodySegments)
            {
                if (enabled)
                {
                    bodyPart.Rigidbody.isKinematic = false;
                    bodyPart.Rigidbody.useGravity = true;
                    bodyPart.Collider.isTrigger = false;
                }
                else
                {
                    bodyPart.Rigidbody.isKinematic = true;
                    bodyPart.Rigidbody.useGravity = false;
                    bodyPart.Collider.isTrigger = true;
                }
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "position", transform.position.ToSaveable() },
                { "rotation", transform.eulerAngles.ToSaveable() },
                { "health", EntityHealth },
            };
        }

        public void OnLoad(JToken data)
        {
            transform.position = data["position"].ToObject<Vector3>();
            transform.eulerAngles = data["rotation"].ToObject<Vector3>();

            int health = (int)data["health"];
            if (health <= 0)
            {
                IsDead = true;
                EntityHealth = health;
                gameObject.SetActive(false);
            }
            else
            {
                InitializeHealth(health, (int)MaxHealth);
            }
        }
    }
}