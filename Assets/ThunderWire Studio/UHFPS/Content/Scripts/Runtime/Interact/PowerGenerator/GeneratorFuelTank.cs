using System.Collections.Generic;
using System.Linq;
using UHFPS.Tools;
using UnityEngine;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class GeneratorFuelTank : MonoBehaviour, IInteractStart, IInteractStop, IInteractTimed
    {
        public PowerGenerator Generator;
        public ItemGuid FuelItem;
        public string FuelProperty = "fuelLiters";

        public float MinRefuelLiters = 1f;
        public MinMax RefuelTime = new(1f, 10f);

        public float MessageTime = 2f;
        public GString NotRequiredMessage;
        public GString NoCanistersMessage;

        public AudioSource AudioSource;
        public SoundClip RefuelSound;
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