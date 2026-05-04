using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
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

        public List<BodySegment> BodySegments = new();
        public List<Component> DisableComponents;

        public Transform Hips;
        public Collider Head;
        public Layer BodyPartLayer;

        public uint MaxHealth = 100;
        public uint StartHealth = 100;
        public float HeadshotMultiplier = 2f;
        public bool AllowHeadhsot = true;

        public bool RemoveCorpse;
        public bool DisableCorpse;
        public float CorpseRemoveTime = 10f;

        public AudioClip[] DamageSounds;
        [Range(0f, 1f)] public float DamageVolume = 1f;

        public SoundClip DeathSound;

        public UnityEvent<int> OnTakeDamage;
        public UnityEvent OnDeath;
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