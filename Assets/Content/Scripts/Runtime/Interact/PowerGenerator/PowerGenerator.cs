using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using ThunderWire.Attributes;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [Summary("Máy phát điện cung cấp năng lượng cho các thiết bị IPowerConsumer dựa trên mức tiêu thụ nhiên liệu (Fuel).")]
    public class PowerGenerator : MonoBehaviour, ISaveable
    {
        [Tooltip("Dung lượng nhiên liệu tối đa (Lít).")]
        public float MaxFuelLiters = 10f;

        [Tooltip("Dung lượng nhiên liệu hiện tại có trong máy.")]
        public float CurrentFuelLiters = 10f;

        [Tooltip("Công tắc để khởi động/tắt máy phát.")]
        public SimpleSwitcher Switcher;

        [Tooltip("Hiệu ứng hạt (Particle) khói xả ra khi máy đang chạy.")]
        public ParticleSystem ExhaustParticles;

        [Tooltip("Thanh trượt (UI) hiển thị mức nhiên liệu còn lại.")]
        public Slider FuelStatus;

        [Range(0.01f, 1f)]
        [Tooltip("Hiệu suất của máy phát điện.")]
        public float GeneratorEfficiency = 0.4f;

        [Tooltip("Năng suất tỏa nhiệt của nhiên liệu (Độ hao hụt điện).")]
        public float FuelCalorificValue = 35f;

        [Tooltip("Lượng nhiên liệu cơ bản (Lít/Giờ) tiêu thụ chỉ để duy trì động cơ (Không tính tải).")]
        public float MotorFuelDrainPerHour = 0.1f;

        [RequireInterface(typeof(IPowerConsumer))]
        [Tooltip("Danh sách các thiết bị sử dụng điện tĩnh (Gắn cứng trên Scene).")]
        public List<MonoBehaviour> PowerConsumers = new();

        [Tooltip("Nguồn âm thanh dùng để trộn (Crossfade).")]
        public AudioSource AudioSourceA;

        [Tooltip("Nguồn âm thanh dùng để trộn (Crossfade).")]
        public AudioSource AudioSourceB;

        [Tooltip("Thời gian trộn (Chuyển dần âm lượng) giữa các âm thanh của động cơ.")]
        public float BlendTime = 0.2f;

        [Tooltip("Âm thanh lặp liên tục khi động cơ đang chạy.")]
        public SoundClip MotorLoop;

        [Tooltip("Âm thanh khi bắt đầu khởi động máy.")]
        public SoundClip MotorStart;

        [Tooltip("Âm thanh khi máy dừng hoạt động.")]
        public SoundClip MotorEnd;

        [Tooltip("Sự kiện gọi ra khi Máy Phát khởi động.")]
        public UnityEvent OnGeneratorStart;

        [Tooltip("Sự kiện gọi ra khi Máy Phát tắt.")]
        public UnityEvent OnGeneratorEnd;

        [Tooltip("Sự kiện gọi ra khi Máy Phát cạn kiệt nhiên liệu.")]
        public UnityEvent OnOutOfFuel;

        private readonly CompositeDisposable disposables = new();
        private readonly Dictionary<IPowerConsumer, IDisposable> runtimeConsumers = new();
        private AudioCrossfader crossFader;

        public float fuelConsumptionRate;
        private bool generatorRunning;

        public int RuntimeConsumers => runtimeConsumers.Count;

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        private void Awake()
        {
            AudioSourceA.loop = false;
            AudioSourceB.loop = false;
            crossFader = new(AudioSourceA, AudioSourceB);
            UpdateFuelStatus();
        }

        private void Start()
        {
            CalculateFuelConsumptionRate();
            foreach (var consumer in PowerConsumers)
            {
                if (consumer is IPowerConsumer obj)
                {
                    IDisposable disposable = obj.IsTurnedOn.Subscribe((_) => CalculateFuelConsumptionRate());
                    disposables.Add(disposable);
                }
            }
        }

        public void AddPowerConsumer(IPowerConsumer consumer)
        {
            IDisposable disposable = consumer.IsTurnedOn.Subscribe((_) => CalculateFuelConsumptionRate());
            runtimeConsumers.Add(consumer, disposable);
        }

        public void RemovePowerConsumer(IPowerConsumer consumer)
        {
            if(runtimeConsumers.TryGetValue(consumer, out var disposable))
            {
                disposable.Dispose();
                runtimeConsumers.Remove(consumer);
                consumer.OnPowerState(false);
                CalculateFuelConsumptionRate();
            }
        }

        public void StartGenerator()
        {
            if (CurrentFuelLiters > 0 && !generatorRunning && !crossFader.IsTransitioning)
            {
                generatorRunning = true;
                StartCoroutine(crossFader.CrossfadeAB(MotorStart, MotorLoop, BlendTime));
                OnGeneratorStart?.Invoke();
            }
        }

        public void StopGenerator()
        {
            if (generatorRunning && !crossFader.IsTransitioning)
            {
                generatorRunning = false;
                StartCoroutine(crossFader.CrossfadeA(MotorEnd, BlendTime));
                OnGeneratorEnd?.Invoke();
            }
        }

        public void SwitchGenerator(bool state)
        {
            if (CurrentFuelLiters <= 0f)
                return;

            if (!generatorRunning && state) StartGenerator();
            else if(generatorRunning && !state) StopGenerator();
            SetGeneratorState(state);
        }

        private void SetGeneratorState(bool state)
        {
            foreach (var consumer in PowerConsumers)
            {
                if (consumer is IPowerConsumer obj)
                    obj.OnPowerState(state);
            }

            foreach (var consumer in runtimeConsumers)
            {
                consumer.Key.OnPowerState(state);
            }

            if (ExhaustParticles != null)
            {
                if (state) ExhaustParticles.Play();
                else ExhaustParticles.Stop();
            }
        }

        public void RefuelGenerator(float liters)
        {
            CurrentFuelLiters += liters;
            if (CurrentFuelLiters > MaxFuelLiters)
                CurrentFuelLiters = MaxFuelLiters;

            UpdateFuelStatus();
        }

        private void CalculateFuelConsumptionRate()
        {
            float totalWatts = 0f;
            foreach (var consumer in PowerConsumers)
            {
                if (consumer is IPowerConsumer obj && obj.IsTurnedOn.Value)
                    totalWatts += obj.ConsumeWattage;
            }

            foreach (var consumer in runtimeConsumers)
            {
                totalWatts += consumer.Key.ConsumeWattage;
            }

            fuelConsumptionRate = (totalWatts * 3600) / (GeneratorEfficiency * FuelCalorificValue * 1000);
            fuelConsumptionRate += MotorFuelDrainPerHour;
        }

        private void UpdateFuelStatus()
        {
            if (FuelStatus == null)
                return;

            float fuelPercent = CurrentFuelLiters / MaxFuelLiters;
            FuelStatus.value = fuelPercent;
        }

        private void Update()
        {
            if (Switcher != null)
                Switcher.IsInteractable = !crossFader.IsTransitioning;

            if (!generatorRunning)
                return;

            CurrentFuelLiters -= fuelConsumptionRate * Time.deltaTime / 3600f;

            if (CurrentFuelLiters <= 0f)
            {
                CurrentFuelLiters = 0f;
                StopGenerator();
                SetGeneratorState(false);
                OnOutOfFuel?.Invoke();
            }

            UpdateFuelStatus();
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "currentfuel", CurrentFuelLiters },
                { "isRunning", generatorRunning }
            };
        }

        public void OnLoad(JToken data)
        {
            CurrentFuelLiters = data["currentfuel"].ToObject<float>();
            bool isRunning = data["isRunning"].ToObject<bool>();

            if (isRunning && CurrentFuelLiters > 0f)
            {
                AudioSourceB.SetSoundClip(MotorLoop);
                AudioSourceB.loop = true;
                AudioSourceB.Play();

                CalculateFuelConsumptionRate();
                SetGeneratorState(true);
                generatorRunning = true;
            }

            UpdateFuelStatus();
        }
    }
}