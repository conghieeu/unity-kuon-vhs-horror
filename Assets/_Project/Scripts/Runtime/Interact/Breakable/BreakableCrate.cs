using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class BreakableCrate : BaseBreakableEntity
    {
        [Serializable]
        public struct CrateItem
        {
            public ObjectReference Item;
            public Percentage Probability;
        }

        public List<CrateItem> CrateItems = new();
        public ObjectReference ItemInside;
        public GameObject BrokenCratePrefab;
        public Transform CrateCenter;

        public bool SpawnRandomItem;
        public bool ShowFloatingIcon;
        public bool EnableItemsGravity;
        public MinMax PiecesKeepTime;
        public Vector3 BrokenRotation;
        public Vector3 SpawnedRotation;

        public bool ExplosionEffect;
        public float UpwardsModifer = 1.5f;
        public float ExplosionPower = 200;
        public float ExplosionRadius = 0.5f;

        public SoundClip BreakSound;
        public UnityEvent OnCrateBreak;

        private FloatingIconModule floatingIcon;

        private void Awake()
        {
            InitializeHealth(100);
            floatingIcon = GameManager.Module<FloatingIconModule>();
        }

        public override void OnBreak()
        {
            gameObject.SetActive(false);

            Vector3 brokenCrateRotation = transform.eulerAngles + BrokenRotation;
            GameObject brokenCrateObj = Instantiate(BrokenCratePrefab, transform.position, Quaternion.Euler(brokenCrateRotation), transform.parent);

            if (SpawnRandomItem)
            {
                ObjectReference randomItem = GetRandomObjectReference();
                if(randomItem != null)
                {
                    GameObject item = SaveGameManager.InstantiateSaveable(randomItem, CrateCenter.position, SpawnedRotation);
                    if(ShowFloatingIcon) floatingIcon.AddFloatingIcon(item);
                    if (EnableItemsGravity)
                    {
                        Rigidbody itemRigidbody = item.GetComponentInChildren<Rigidbody>();
                        itemRigidbody.isKinematic = false;
                        itemRigidbody.useGravity = true;
                    }
                }
            }
            else
            {
                GameObject item = SaveGameManager.InstantiateSaveable(ItemInside, CrateCenter.position, SpawnedRotation);
                if (ShowFloatingIcon) floatingIcon.AddFloatingIcon(item);
                if (EnableItemsGravity)
                {
                    Rigidbody itemRigidbody = item.GetComponentInChildren<Rigidbody>();
                    itemRigidbody.isKinematic = false;
                    itemRigidbody.useGravity = true;
                }
            }

            float maxDestroyTime = 0;
            foreach (var brokenPiece in brokenCrateObj.GetComponentsInChildren<Rigidbody>())
            {
                float destroyTime = PiecesKeepTime.Random();
                Destroy(brokenPiece.gameObject, destroyTime);

                if (destroyTime > maxDestroyTime)
                    maxDestroyTime = destroyTime;

                if (ExplosionEffect && CrateCenter)
                {
                    brokenPiece.AddExplosionForce(ExplosionPower, CrateCenter.position, ExplosionRadius, UpwardsModifer);
                }
            }

            Destroy(brokenCrateObj, maxDestroyTime);
            GameTools.PlayOneShot3D(transform.position, BreakSound, "CrateBreakSound");
            OnCrateBreak?.Invoke();
        }

        private ObjectReference GetRandomObjectReference()
        {
            int poolSize = 0;
            foreach (var item in CrateItems)
            {
                poolSize += item.Probability;
            }

            System.Random random = new();
            int randomNumber = random.Next(0, poolSize) + 1;
            int accumulatedProbability = 0;

            foreach (var item in CrateItems)
            {
                accumulatedProbability += item.Probability;
                if (randomNumber <= accumulatedProbability)
                    return item.Item;
            }

            return null;
        }

        public override StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(EntityHealth), EntityHealth },
                { nameof(isBroken), isBroken }
            };
        }

        public override void OnLoad(JToken data)
        {
            int health = (int)data[nameof(EntityHealth)];
            isBroken = (bool)data[nameof(isBroken)];

            InitializeHealth(health);
            if (isBroken) gameObject.SetActive(false);
        }
    }
}