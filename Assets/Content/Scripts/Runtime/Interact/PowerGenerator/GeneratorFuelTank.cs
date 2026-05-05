using System.Collections.Generic;
using System.Linq;
using UHFPS.Tools;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    [Summary("Quản lý bình xăng/dầu của máy phát điện. Cần đổ nhiên liệu (Hold) để duy trì hoạt động.")]
    public class GeneratorFuelTank : MonoBehaviour, IInteractStart, IInteractStop, IInteractTimed
    {
        [Tooltip("Tham chiếu tới máy phát điện liên kết.")]
        public PowerGenerator Generator;

        [Tooltip("Vật phẩm (Can dầu/xăng) yêu cầu để đổ.")]
        public ItemGuid FuelItem;

        [Tooltip("Tên biến lưu trữ lượng nhiên liệu của vật phẩm trong Json (Mặc định: fuelLiters).")]
        public string FuelProperty = "fuelLiters";

        [Tooltip("Lượng nhiên liệu tối thiểu còn thiếu để cho phép bắt đầu đổ.")]
        public float MinRefuelLiters = 1f;

        [Tooltip("Thời gian (giây) cần giữ nút để đổ đầy toàn bộ dung tích.")]
        public MinMax RefuelTime = new(1f, 10f);

        [Tooltip("Thời gian hiển thị tin nhắn gợi ý trên màn hình.")]
        public float MessageTime = 2f;

        [Tooltip("Nội dung thông báo khi bình nhiên liệu đã đầy.")]
        public GString NotRequiredMessage;

        [Tooltip("Nội dung thông báo khi người chơi không có can dầu.")]
        public GString NoCanistersMessage;

        [Tooltip("Nguồn phát âm thanh (AudioSource) khi đổ.")]
        public AudioSource AudioSource;

        [Tooltip("Âm thanh đổ nhiên liệu (Chất lỏng).")]
        public SoundClip RefuelSound;

        [Tooltip("Thời gian làm mờ (Fade) âm lượng khi kết thúc đổ.")]
        public float FadeTime;

        public float InteractTime { get; set; }

        private readonly Dictionary<InventoryItem, float> requiredCanisters = new();
        private AudioCrossfader crossfader;
        private GameManager gameManager;
        private Inventory inventory;

        private float refuelLiters;
        private bool canRefuel;

        public bool NoInteract
        {
            get
            {
                if (Inventory.HasReference)
                    return !canRefuel || !Inventory.Instance.ContainsItem(FuelItem);

                return true;
            }
        }

        private void Awake()
        {
            crossfader = new AudioCrossfader(AudioSource);
            gameManager = GameManager.Instance;
            inventory = Inventory.Instance;
        }

        private void Start()
        {
            NotRequiredMessage.SubscribeGloc();
            NoCanistersMessage.SubscribeGloc();
        }

        public void InteractStart()
        {
            if (!Inventory.HasReference)
                return;

            requiredCanisters.Clear();
            float maxLiters = Generator.MaxFuelLiters;
            float currentLiters = Generator.CurrentFuelLiters;
            float toFullLiters = maxLiters - currentLiters;
            float remainingLiters = toFullLiters;

            bool containsCanisters = false;
            if (Inventory.Instance.ContainsItemMany(FuelItem, out var items))
            {
                containsCanisters = true;
                Dictionary<InventoryItem, float> itemWithLiters = new();

                foreach (var item in items)
                {
                    var json = item.inventoryItem.CustomData.GetJson();
                    if (json.ContainsKey(FuelProperty))
                    {
                        float liters = json[FuelProperty].ToObject<float>();
                        itemWithLiters.Add(item.inventoryItem, liters);
                    }
                }

                Dictionary<InventoryItem, float> sortedCanisters = itemWithLiters
                    .OrderBy(x => x.Value).ToDictionary(x => x.Key, y => y.Value);

                foreach (var item in sortedCanisters)
                {
                    if (remainingLiters <= 0)
                        break;

                    float remainder = Mathf.Clamp(item.Value - remainingLiters, 0f, Mathf.Infinity);
                    remainingLiters = Mathf.Clamp(remainingLiters - item.Value, 0f, Mathf.Infinity);
                    requiredCanisters.Add(item.Key, remainder);
                }
            }

            if (containsCanisters && (canRefuel = toFullLiters > MinRefuelLiters))
            {
                float toRefuel = Mathf.Clamp(toFullLiters - remainingLiters, 0f, Mathf.Infinity);
                float t = Mathf.InverseLerp(0f, maxLiters, toRefuel);
                InteractTime = Mathf.Lerp(RefuelTime.RealMin, RefuelTime.RealMax, t);
                refuelLiters = toRefuel;

                StartCoroutine(crossfader.FadeIn(RefuelSound, FadeTime, true));
            }
            else
            {
                if (!containsCanisters) gameManager.ShowHintMessage(NoCanistersMessage, MessageTime);
                else if (requiredCanisters.Count <= 0) gameManager.ShowHintMessage(NotRequiredMessage, MessageTime);
            }
        }

        public void InteractStop()
        {
            StartCoroutine(crossfader.FadeOut(FadeTime));
        }

        public void InteractTimed()
        {
            Generator.RefuelGenerator(refuelLiters);
            StartCoroutine(crossfader.FadeOut(FadeTime));
            refuelLiters = 0f;

            foreach (var item in requiredCanisters)
            {
                if(item.Value > 0)
                {
                    var json = item.Key.CustomData.GetJson();
                    json[FuelProperty] = item.Value;
                    item.Key.CustomData.Update(json);
                }
                else
                {
                    inventory.RemoveItem(item.Key);
                }
            }
        }
    }
}