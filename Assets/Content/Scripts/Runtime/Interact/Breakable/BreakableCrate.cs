using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Breakable Crate")]
    [Summary("Quản lý một thùng gỗ (Crate) hoặc vật thể có thể bị đập vỡ, rơi ra vật phẩm bên trong.")]
    public class BreakableCrate : BaseBreakableEntity
    {
        [Serializable]
        public struct CrateItem
        {
            public ObjectReference Item;
            public Percentage Probability;
        }

        [Tooltip("Danh sách các vật phẩm có thể rơi ra theo xác suất ngẫu nhiên.")]
        public List<CrateItem> CrateItems = new();

        [Tooltip("Vật phẩm cố định được giấu bên trong (Chỉ dùng nếu không sinh đồ ngẫu nhiên).")]
        public ObjectReference ItemInside;

        [Tooltip("Prefab của vật thể đã bị vỡ (chứa các mảnh vỡ vật lý).")]
        public GameObject BrokenCratePrefab;

        [Tooltip("Vị trí trung tâm (Nơi sinh ra vật phẩm khi vỡ).")]
        public Transform CrateCenter;

        [Tooltip("Đánh dấu để chọn sinh vật phẩm ngẫu nhiên từ danh sách CrateItems.")]
        public bool SpawnRandomItem;

        [Tooltip("Hiện Icon nổi trên màn hình cho vật phẩm rớt ra.")]
        public bool ShowFloatingIcon;

        [Tooltip("Bật Vật lý (Gravity) cho vật phẩm rớt ra.")]
        public bool EnableItemsGravity;

        [Tooltip("Khoảng thời gian (giây) ngẫu nhiên để xóa dần các mảnh vỡ vật lý.")]
        public MinMax PiecesKeepTime;

        [Tooltip("Góc xoay cộng thêm cho mô hình vỡ.")]
        public Vector3 BrokenRotation;

        [Tooltip("Góc xoay của vật phẩm rớt ra.")]
        public Vector3 SpawnedRotation;

        [Tooltip("Bật hiệu ứng văng (Explosion) để các mảnh vỡ bắn ra xung quanh.")]
        public bool ExplosionEffect;
        public float UpwardsModifer = 1.5f;
        public float ExplosionPower = 200;
        public float ExplosionRadius = 0.5f;

        [Tooltip("Âm thanh phát ra khi vật thể bị vỡ.")]
        public SoundClip BreakSound;

        [Tooltip("Sự kiện kích hoạt khi vật thể bị vỡ.")]
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