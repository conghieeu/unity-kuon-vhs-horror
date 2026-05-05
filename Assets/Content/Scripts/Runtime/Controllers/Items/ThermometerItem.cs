using System.Globalization;
using System.Collections;
using System;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Điều khiển vật phẩm Nhiệt kế (Thermometer), đo và hiển thị nhiệt độ của môi trường hoặc đối tượng bất thường.")]
    public class ThermometerItem : PlayerItemBehaviour
    {
        [Header("Display Settings")]
        [Tooltip("Canvas chứa giao diện hiển thị trên màn hình nhiệt kế.")]
        public GameObject DisplayCanvas;
        [Tooltip("Thành phần Text (TextMeshPro) hiển thị giá trị nhiệt độ.")]
        public TMP_Text Temperature;
        [Tooltip("Định dạng chuỗi hiển thị cho giá trị nhiệt độ (ví dụ: số nguyên và số thập phân).")]
        public string DisplayFormat = "<mspace=0.5em>{0}</mspace>.<mspace=0.5em>{1}</mspace>";

        [Tooltip("Vật liệu (Material) của màn hình hiển thị nhiệt độ.")]
        public RendererMaterial Display;
        [Tooltip("Từ khóa Shader bật phát sáng (Emission) khi màn hình đang bật.")]
        public string EmissionKeyword = "_EMISSION";

        [Header("Temperature Settings")]
        [Tooltip("Khởi tạo nhiệt độ cơ bản khi bắt đầu trò chơi.")]
        public bool SetBaseTemp = true;
        [Tooltip("Nhiệt độ cơ bản (mặc định) của môi trường (độ C).")]
        public float BaseTemperature = 26f;

        [Header("Detection Settings")]
        [Tooltip("Lớp mạng (LayerMask) của đối tượng phát nhiệt cần đo.")]
        public LayerMask RaycastMask;
        [Tooltip("Khoảng cách tối đa (tia Raycast) để quét và đo nhiệt độ đối tượng.")]
        public float RaycastDistance;

        [Header("Simulation Settings")]
        [Tooltip("Khoảng thời gian (interval) làm mới nhiệt độ hiển thị.")]
        public float TempGetInterval;
        [Tooltip("Biên độ biến đổi (Noise) ngẫu nhiên làm nhiệt độ hiển thị thay đổi tự nhiên.")]
        public float TempNoiseScale;
        [Tooltip("Tốc độ thay đổi của độ nhiễu (Noise).")]
        public float TempNoiseSpeed;

        [Tooltip("Tốc độ tăng nhiệt độ trên màn hình để tiệm cận giá trị thực.")]
        public float TempGainSpeed;
        [Tooltip("Tốc độ giảm nhiệt độ trên màn hình để tiệm cận giá trị thực.")]
        public float TempDropSpeed;

        [Header("Animations")]
        [Tooltip("Tên trạng thái hoạt ảnh khi lấy nhiệt kế ra.")]
        public string ThermometerDrawState = "ThermometerDraw";
        [Tooltip("Tên trạng thái hoạt ảnh khi cất nhiệt kế đi.")]
        public string ThermometerHideState = "ThermometerHide";
        [Tooltip("Tham số Trigger gọi hoạt ảnh cất nhiệt kế.")]
        public string ThermometerHideTrigger = "Hide";

        private float tempInterval;
        private float targetTemp;
        private float currTemp;

        private float baseTemp;
        private float defaultTemp;

        private bool isEquipped;
        private bool tempEnabled;

        public override string Name => "Thermometer";

        public override bool IsBusy() => !isEquipped;

        public override bool CanCombine() => isEquipped;

        private void Awake()
        {
            if(!SaveGameManager.GameWillLoad && SetBaseTemp)
                SetResetTemp(BaseTemperature);
        }

        public void SetResetTemp(float temperature)
        {
            baseTemp = temperature;
            currTemp = temperature;
            targetTemp = temperature;
            defaultTemp = temperature;
        }

        public void SetTemperature(float temperature)
        {
            targetTemp = temperature;
        }

        public void SetBaseTemperature(float baseTemperature)
        {
            baseTemp = baseTemperature;
        }

        public void ResetTemperature()
        {
            baseTemp = defaultTemp;
            targetTemp = defaultTemp;
        }

        public override void OnUpdate()
        {
            if (!tempEnabled)
                return;

            if (Physics.Raycast(CameraRay, out RaycastHit hit, RaycastDistance, RaycastMask))
            {
                if (hit.collider.TryGetComponent(out ThermometerTemp temp))
                {
                    if(!temp.IsBaseTrigger)
                        targetTemp = temp.Temperature;
                }
                else
                {
                    targetTemp = baseTemp;
                }
            }
            else
            {
                targetTemp = baseTemp;
            }

            if (tempInterval > 0) tempInterval -= Time.deltaTime;
            float noise = Mathf.PerlinNoise1D(Time.time * TempNoiseSpeed) * TempNoiseScale;

            if(currTemp > targetTemp) currTemp = Mathf.Lerp(currTemp, targetTemp, Time.deltaTime * TempGainSpeed);
            else currTemp = Mathf.Lerp(currTemp, targetTemp, Time.deltaTime * TempDropSpeed);

            if (tempInterval <= 0)
            {
                float temp = currTemp + noise;
                DisplayTemperature(temp);
                tempInterval = TempGetInterval;
            }
        }

        private void DisplayTemperature(float temp)
        {
            double roundedNumber = Math.Round(temp, 1);
            string mgText = roundedNumber.ToString("0.0", CultureInfo.InvariantCulture);

            if (mgText.Contains('.'))
            {
                string[] parts = mgText.Split('.');
                string wholePart = parts[0].TrimStart('0');

                if (string.IsNullOrEmpty(wholePart))
                    wholePart = "0";

                string final = string.Format(DisplayFormat, wholePart, parts[1]);
                Temperature.text = final;
            }
            else
            {
                string final = string.Format(DisplayFormat, 0, 0);
                Temperature.text = final;
            }
        }

        private void SetDisplay(bool state)
        {
            if (state)
            {
                Display.ClonedMaterial.EnableKeyword(EmissionKeyword);
                DisplayCanvas.SetActive(true);
            }
            else
            {
                Display.ClonedMaterial.DisableKeyword(EmissionKeyword);
                DisplayCanvas.SetActive(false);
            }
        }

        public override void OnItemSelect()
        {
            tempEnabled = true;
            ItemObject.SetActive(true);
            SetDisplay(true);

            StartCoroutine(ShowThermometer());
        }

        IEnumerator ShowThermometer()
        {
            yield return new WaitForAnimatorClip(Animator, ThermometerDrawState);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            StopAllCoroutines();
            StartCoroutine(HideThermometer());

            SetDisplay(false);
            tempEnabled = false;
            Animator.SetTrigger(ThermometerHideTrigger);
        }

        IEnumerator HideThermometer()
        {
            yield return new WaitForAnimatorClip(Animator, ThermometerHideState);

            ItemObject.SetActive(false);
            isEquipped = false;
        }

        public override void OnItemActivate()
        {
            ItemObject.SetActive(true);
            Animator.Play(ThermometerDrawState, 0, 1f);
            SetDisplay(true);

            tempEnabled = true;
            isEquipped = true;
        }

        public override void OnItemDeactivate()
        {
            ItemObject.SetActive(false);
            SetDisplay(false);

            tempEnabled = false;
            isEquipped = false;
        }

        public override StorableCollection OnCustomSave()
        {
            return new StorableCollection()
            {
                { nameof(baseTemp), baseTemp }
            };
        }

        public override void OnCustomLoad(JToken data)
        {
            float baseTemp = (float)data[nameof(baseTemp)];
            SetResetTemp(baseTemp);
        }
    }
}