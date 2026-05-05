using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UHFPS.Input;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Điều khiển vật phẩm Máy Quay Phim (Camera), bao gồm các chức năng quay phim, quay đêm (Night Vision), thu phóng và quản lý pin.")]
    public class CameraItem : PlayerItemBehaviour
    {
        [Header("Camera Settings")]
        [Tooltip("Vật phẩm Pin trong Inventory dùng để nạp lại năng lượng cho máy quay.")]
        public ItemGuid BatteryInventoryItem;
        [Tooltip("Nguồn phát âm thanh (AudioSource) của máy quay.")]
        public AudioSource CameraAudio;
        [Tooltip("Đèn rọi của máy quay (dùng khi bật Night Vision).")]
        public Light CameraLight;

        [Header("Night Vision Settings")]
        [Tooltip("Thành phần Post-Processing dùng để tạo hiệu ứng quay đêm (Night Vision).")]
        public VolumeComponentReferecne NVComponent;
        [Tooltip("Tỉ lệ phần trăm pin ban đầu của máy quay.")]
        public Percentage BatteryPercentage = 100;
        [Tooltip("Mức phần trăm cảnh báo pin yếu.")]
        public Percentage BatteryLowPercent = 20;
        [Tooltip("Máy quay có tiêu hao pin khi không bật Night Vision hay không? (Nếu true, chỉ hao pin khi bật NV).")]
        public bool NoNVDrainBattery = false;
        [Tooltip("Trạng thái mặc định của Night Vision khi bắt đầu game.")]
        public bool InitialNVState = false;

        [Header("Battery Settings")]
        [Tooltip("Tốc độ hao pin khi bật Night Vision.")]
        public float HighBatteryDrainSpeed = 2f;
        [Tooltip("Tốc độ hao pin bình thường (khi tắt Night Vision).")]
        public float LowBatteryDrainSpeed = 1f;

        [Tooltip("Cường độ sáng tối đa của đèn máy quay.")]
        public float LightIntensity = 1f;
        [Tooltip("Màu sắc của biểu tượng pin khi đầy.")]
        public Color BatteryFullColor = Color.white;
        [Tooltip("Màu sắc của biểu tượng pin khi yếu.")]
        public Color BatteryLowColor = Color.red;

        [Header("Zoom Settings")]
        [Tooltip("Tầm xa của đèn rọi khi thu phóng máy quay tối đa.")]
        public float LightZoomRange = 24;
        [Tooltip("Góc nhìn (FOV) khi thu phóng máy quay tối đa.")]
        public float CameraZoomFOV = 30f;
        [Tooltip("Tốc độ thu phóng của máy quay.")]
        public float CameraZoomSpeed = 5f;

        [Header("Audio Channels")]
        [Tooltip("Kích hoạt kênh hiển thị âm lượng thanh gạt trái/phải.")]
        public bool EnableChannels = true;
        [Tooltip("Độ dài dữ liệu âm thanh lấy mẫu.")]
        public int SampleDataLength = 1024;
        [Tooltip("Độ trễ số khung hình giữa các lần cập nhật UI âm lượng.")]
        public int FrameDelay = 10;
        [Tooltip("Giá trị RMS tối đa để tính toán mức độ âm lượng.")]
        public float MaxRMSValue = 0.1f;

        [Header("Animations")]
        [Tooltip("Tên trạng thái hoạt ảnh khi lấy máy quay ra.")]
        public string CameraShow = "CameraShow";
        [Tooltip("Tên trạng thái hoạt ảnh khi cất máy quay đi.")]
        public string CameraHide = "CameraHide";
        [Tooltip("Tên trạng thái hoạt ảnh khi thay pin máy quay.")]
        public string CameraReload = "CameraReload";

        [Header("Fades")]
        [Tooltip("Thời gian bù trừ (offset) để làm mờ dần khi mở máy quay.")]
        public float CameraShowFadeOffset = 0.35f;
        [Tooltip("Thời gian bù trừ (offset) để làm mờ dần khi cất máy quay.")]
        public float CameraHideFadeOffset = 0.1f;
        [Tooltip("Tốc độ làm mờ (Fade In) màn hình khi mở máy quay.")]
        public float CameraShowFadeSpeed = 3f;
        [Tooltip("Tốc độ làm mờ (Fade Out) màn hình khi cất máy quay.")]
        public float CameraHideFadeSpeed = 3f;

        [Header("Sounds")]
        [Tooltip("Âm thanh khi trang bị máy quay.")]
        public SoundClip CameraEquip;
        [Tooltip("Âm thanh khi cất máy quay.")]
        public SoundClip CameraUnequip;
        [Tooltip("Âm thanh khi bắt đầu thu phóng máy quay (Zoom In).")]
        public SoundClip CameraZoomIn;
        [Tooltip("Âm thanh khi nhả thu phóng máy quay (Zoom Out).")]
        public SoundClip CameraZoomOut;
        [Tooltip("Âm thanh khi bật/tắt chế độ Night Vision.")]
        public SoundClip CameraNVSwitch;

        private GameManager gameManager;

        private GameObject cameraOverlay;
        private TMPro.TMP_Text recordingText;
        private Slider cameraZoom;
        private Image batteryIcon;
        private Image batteryFill;
        private Image nightVisionIcon;
        private CamcorderChannels channels;

        private CustomStopwatch stopwatch = new();

        private bool isNVEnabled;
        private bool isEquipped;
        private bool isBusy;
        private bool isZoomed;

        private float defaultLightRange;
        private float defaultZoom;
        private float currentZoom;

        public float batteryEnergy;
        private float drainSpeed;
        private float currentBattery;
        private Color batteryColor;

        private float[] audioDataLeft;
        private float[] audioDataRight;
        private int currentFrameCount;

        public override string Name => "Camera";

        public override bool IsBusy() => isBusy;
        public override bool IsEquipped() => ItemObject.activeSelf || isEquipped;
        public override bool CanCombine() => isEquipped && !isBusy;

        private void Awake()
        {
            gameManager = GameManager.Instance;

            var behaviours = gameManager.GraphicReferences.Value["Camera"];
            cameraOverlay = behaviours[0].gameObject;
            cameraZoom = (Slider)behaviours[1];
            recordingText = (TMPro.TMP_Text)behaviours[2];
            batteryIcon = (Image)behaviours[3];
            batteryFill = batteryIcon.transform.GetChild(0).GetComponent<Image>();
            nightVisionIcon = (Image)behaviours[4];
            channels = (CamcorderChannels)behaviours[6];

            defaultZoom = PlayerManager.MainVirtualCamera.Lens.FieldOfView;
            defaultLightRange = CameraLight.range;
            currentZoom = defaultZoom;

            if (!SaveGameManager.GameWillLoad)
            {
                isNVEnabled = InitialNVState;
                SetNVState(isNVEnabled, true);

                currentBattery = BatteryPercentage;
                UpdateBattery();

                batteryColor = batteryEnergy > BatteryLowPercent.Ratio()
                    ? BatteryFullColor : BatteryLowColor;

                batteryIcon.color = batteryColor;
                batteryFill.color = batteryColor;
            }

            if (EnableChannels)
            {
                audioDataLeft = new float[SampleDataLength];
                audioDataRight = new float[SampleDataLength];
            }
            else
            {
                channels.gameObject.SetActive(false);
            }
        }

        public override void OnUpdate()
        {
            if (!isEquipped) return;

            UpdateRecordingTime();

            if (isBusy) return;

            // camera zoom
            float zoomT = Mathf.InverseLerp(defaultZoom, CameraZoomFOV, currentZoom);
            if (InputManager.ReadButton(Controls.ADS))
            {
                currentZoom = Mathf.MoveTowards(currentZoom, CameraZoomFOV, Time.deltaTime * CameraZoomSpeed * 10);
                if (!isBusy)
                {
                    if (zoomT > 0 && zoomT < 1)
                    {
                        CameraAudio.SetSoundClip(CameraZoomIn, play: true);
                        isZoomed = true;
                    }
                    else if (zoomT == 1 && isZoomed)
                    {
                        CameraAudio.Stop();
                        isZoomed = false;
                    }
                }
            }
            else
            {
                currentZoom = Mathf.MoveTowards(currentZoom, defaultZoom, Time.deltaTime * CameraZoomSpeed * 10);
                if (!isBusy)
                {
                    if (zoomT > 0 && zoomT < 1)
                    {
                        CameraAudio.SetSoundClip(CameraZoomOut, play: true);
                        isZoomed = true;
                    }
                    else if (zoomT == 0 && isZoomed)
                    {
                        CameraAudio.Stop();
                        isZoomed = false;
                    }
                }
            }

            PlayerManager.MainVirtualCamera.Lens.FieldOfView = currentZoom;
            CameraLight.range = Mathf.Lerp(defaultLightRange, LightZoomRange, zoomT);
            cameraZoom.value = zoomT;

            // night vision
            if (InputManager.ReadButtonOnce(this, Controls.FLASHLIGHT))
            {
                if (isNVEnabled = !isNVEnabled)
                {
                    drainSpeed = HighBatteryDrainSpeed;
                    SetNVState(true);
                }
                else
                {
                    drainSpeed = LowBatteryDrainSpeed;
                    SetNVState(false);
                }
            }

            UpdateChannels();

            // battery life
            currentBattery = currentBattery > 0 ? currentBattery -= Time.deltaTime * drainSpeed : 0;
            UpdateBattery();

            // battery icon
            batteryColor = batteryEnergy > BatteryLowPercent.Ratio()
                ? Color.Lerp(batteryColor, BatteryFullColor, Time.deltaTime * 10)
                : Color.Lerp(batteryColor, BatteryLowColor, Time.deltaTime * 10);

            batteryIcon.color = batteryColor;
            batteryFill.color = batteryColor;
        }

        private void UpdateRecordingTime()
        {
            TimeSpan timeSpan = stopwatch.Elapsed;
            recordingText.text = timeSpan.ToString(@"hh\:mm\:ss\:ff");
        }

        private void UpdateBattery()
        {
            batteryEnergy = Mathf.InverseLerp(0, BatteryPercentage, currentBattery);
            batteryFill.fillAmount = batteryEnergy;
            CameraLight.intensity = Mathf.Lerp(0, LightIntensity, batteryEnergy);
        }

        private void UpdateChannels()
        {
            if (!EnableChannels)
                return;

            if (currentFrameCount == FrameDelay)
            {
                CalculateRMS();
                currentFrameCount = 0;
            }
            else
            {
                currentFrameCount++;
            }
        }

        private void CalculateRMS()
        {
            AudioListener.GetOutputData(audioDataLeft, 0);
            AudioListener.GetOutputData(audioDataRight, 1);

            float sumLeft = 0;
            float sumRight = 0;

            for (int i = 0; i < SampleDataLength; i++)
            {
                sumLeft += audioDataLeft[i] * audioDataLeft[i];
                sumRight += audioDataRight[i] * audioDataRight[i];
            }

            float rmsLeft = Mathf.Sqrt(sumLeft / SampleDataLength) / MaxRMSValue;
            float rmsRight = Mathf.Sqrt(sumRight / SampleDataLength) / MaxRMSValue;

            rmsLeft = Mathf.Clamp(rmsLeft, 0, 1);
            rmsRight = Mathf.Clamp(rmsRight, 0, 1);

            channels.SetChannelValue(CamcorderChannels.Channel.Left, rmsLeft);
            channels.SetChannelValue(CamcorderChannels.Channel.Right, rmsRight);
        }

        public override void OnItemCombine(InventoryItem combineItem)
        {
            if (combineItem.ItemGuid != BatteryInventoryItem || !isEquipped)
                return;

            Inventory.Instance.RemoveItem(combineItem, 1);
            StartCoroutine(ReloadCameraBattery());
            isBusy = true;
        }

        IEnumerator ReloadCameraBattery()
        {
            yield return gameManager.StartBackgroundFade(false);

            ItemObject.SetActive(true);
            SetCameraEffects(false);
            cameraOverlay.SetActive(false);

            PlayerManager.MainVirtualCamera.Lens.FieldOfView = defaultZoom;
            CameraLight.range = defaultLightRange;
            cameraZoom.value = 0f;
            currentZoom = defaultZoom;
            CameraAudio.Stop();

            Animator.SetTrigger(CameraReload);
            yield return new WaitForSeconds(CameraHideFadeOffset);
            StartCoroutine(gameManager.StartBackgroundFade(true));

            yield return new WaitForAnimatorClip(Animator, CameraReload, CameraShowFadeOffset);

            currentBattery = BatteryPercentage;
            UpdateBattery();

            yield return gameManager.StartBackgroundFade(false);

            ItemObject.SetActive(false);
            SetCameraEffects(true);
            cameraOverlay.SetActive(true);

            yield return gameManager.StartBackgroundFade(true);
            isBusy = false;
        }

        public override void OnItemSelect()
        {
            if (isBusy || isEquipped)
                return;

            ItemObject.SetActive(true);
            StartCoroutine(ShowCamera());
            CameraAudio.SetSoundClip(CameraEquip, play: true);
            isBusy = true;
        }

        IEnumerator ShowCamera()
        {
            yield return new WaitForAnimatorClip(Animator, CameraShow, CameraShowFadeOffset);
            yield return gameManager.StartBackgroundFade(false);

            ItemObject.SetActive(false);
            SetCameraEffects(true);
            cameraOverlay.SetActive(true);

            yield return gameManager.StartBackgroundFade(true);
            stopwatch.Start();
            isEquipped = true;
            isBusy = false;
        }

        public override void OnItemDeselect()
        {
            if (isBusy || !isEquipped)
                return;

            CameraAudio.SetSoundClip(CameraUnequip, play: true);
            StartCoroutine(HideCamera());
            isBusy = true;
        }

        IEnumerator HideCamera()
        {
            yield return gameManager.StartBackgroundFade(false);

            ItemObject.SetActive(true);
            SetCameraEffects(false);
            Animator.SetTrigger(CameraHide);
            cameraOverlay.SetActive(false);
            stopwatch.Stop();

            PlayerManager.MainVirtualCamera.Lens.FieldOfView = defaultZoom;
            CameraLight.range = defaultLightRange;
            cameraZoom.value = 0f;
            currentZoom = defaultZoom;
            CameraAudio.Stop();

            yield return new WaitForSeconds(CameraHideFadeOffset);
            StartCoroutine(gameManager.StartBackgroundFade(true));
            yield return new WaitForAnimatorClip(Animator, CameraHide);

            ItemObject.SetActive(false);
            isEquipped = false;
            isBusy = false;
        }

        private void SetCameraEffects(bool state)
        {
            NVComponent.Volume.gameObject.SetActive(state);
            CameraLight.gameObject.SetActive(state);
        }

        private void SetNVState(bool state, bool isInitial = false)
        {
            if(!isInitial) CameraAudio.PlayOneShotSoundClip(CameraNVSwitch);
            int index = NVComponent.ComponentIndex;
            NVComponent.Volume.profile.components[index].active = state;
            drainSpeed = state ? HighBatteryDrainSpeed : NoNVDrainBattery ? LowBatteryDrainSpeed : 0;
            CameraLight.enabled = state;
            nightVisionIcon.Alpha(state ? 1f : 0.25f);
        }

        public override void OnItemActivate()
        {
            cameraOverlay.SetActive(true);
            SetCameraEffects(true);

            stopwatch.Start();
            ItemObject.SetActive(false);
            isEquipped = true;
            isBusy = false;
        }

        public override void OnItemDeactivate()
        {
            cameraOverlay.SetActive(false);
            SetCameraEffects(false);

            stopwatch.Stop();
            ItemObject.SetActive(false);
            isEquipped = false;
            isBusy = false;
        }

        public override StorableCollection OnCustomSave()
        {
            return new StorableCollection()
            {
                { "recordingTime", stopwatch.ElapsedTicks },
                { "batteryEnergy", currentBattery },
                { "nightVision", isNVEnabled },
            };
        }

        public override void OnCustomLoad(JToken data)
        {
            long ticks = data["recordingTime"].ToObject<long>();
            TimeSpan stopwatchOffset = TimeSpan.FromTicks(ticks);
            stopwatch = new CustomStopwatch(stopwatchOffset);

            currentBattery = data["batteryEnergy"].ToObject<float>();
            isNVEnabled = data["nightVision"].ToObject<bool>();

            SetNVState(isNVEnabled, true);
            UpdateBattery();

            batteryColor = batteryEnergy > BatteryLowPercent.Ratio()
                ? BatteryFullColor : BatteryLowColor;

            batteryIcon.color = batteryColor;
            batteryFill.color = batteryColor;
        }
    }
}