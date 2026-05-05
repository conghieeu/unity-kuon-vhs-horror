using System.Collections.Generic;
using System.Globalization;
using System.Collections;
using System.Linq;
using System;
using UnityEngine;
using UHFPS.Tools;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Điều khiển vật phẩm Máy đo điện từ trường (EMF Meter), giúp dò tìm điểm bất thường (Anomaly).")]
    public class EMFMeterItem : PlayerItemBehaviour
    {
        [Header("Indicators")]
        [Tooltip("Danh sách các đèn báo hiệu mức độ EMF trên máy đo.")]
        public List<SkinnedMeshRenderer> Indicators = new();
        [Tooltip("Từ khóa Shader để bật/tắt phát sáng (Emission) cho đèn báo.")]
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Vật liệu (Material) màn hình hiển thị của máy đo.")]
        public RendererMaterial Display;
        [Tooltip("Canvas chứa Text hiển thị giá trị đo được.")]
        public GameObject DisplayCanvas;

        [Tooltip("Văn bản TextMeshPro hiển thị số Milligauss.")]
        public TMP_Text MilligaussText;
        [Tooltip("Định dạng hiển thị giá trị Milligauss.")]
        public string DisplayFormat = "<mspace=0.5em>{0}</mspace>.<mspace=0.5em>{1}</mspace>";

        [Header("Detection")]
        [Tooltip("Lớp mạng (LayerMask) chứa các đối tượng có điểm bất thường (Anomaly).")]
        public LayerMask DetectionMask;
        [Tooltip("Bán kính vùng phát hiện điểm bất thường.")]
        public float DetectionRadius;

        [Header("EMF Settings")]
        [Tooltip("Giá trị Milligauss tối đa máy có thể hiển thị.")]
        public float MaxMilligaussValue = 20f;
        [Range(0f, 1f)]
        [Tooltip("Hệ số bù trừ hướng góc để tính toán mức độ hiển thị khi mục tiêu ở rìa camera.")]
        public float AnomalyDotRangeCompensation = 0.5f;
        [Range(0f, 1f)]
        [Tooltip("Góc nhìn tối thiểu để bắt đầu phát hiện điểm bất thường.")]
        public float MinAnomalyDirection = 0.2f;
        [Tooltip("Tốc độ cập nhật giá trị Milligauss trên màn hình.")]
        public float MilligaussUpdateSpeed = 10f;
        [Tooltip("Tốc độ cập nhật phần thập phân của giá trị Milligauss.")]
        public float DecimalPartUpdateSpeed = 10f;

        [Header("Noise Settings")]
        [Tooltip("Bật tiếng nhiễu nền khi không phát hiện ra EMF.")]
        public bool EnableNoise = true;
        [Tooltip("Mức độ nhiễu nền cơ bản.")]
        public float BackgroundNoise = 0.85f;
        [Tooltip("Biên độ dao động của nhiễu nền.")]
        public float NoiseAmount = 1.0f;
        [Tooltip("Tốc độ thay đổi của tiếng nhiễu nền.")]
        public float NoiseSpeed = 1.0f;

        [Header("Audio Settings")]
        [Tooltip("Bật tiếng bíp của máy đo.")]
        public bool EneableReaderBeep = true;
        [Tooltip("Bật thay đổi cao độ (pitch) tiếng bíp khi mức EMF tăng cao.")]
        public bool EneablePitchedBeep = true;
        [Tooltip("Nguồn phát âm thanh tiếng bíp của máy đo.")]
        public AudioSource ReaderAudio;
        [Tooltip("Mức độ đèn báo tối thiểu để bắt đầu phát tiếng bíp.")]
        public int ReaderStartLevel = 1;
        [Tooltip("Giới hạn cao độ của tiếng bíp (Min-Max).")]
        public MinMax ReaderPitchLimits = new(1f, 1.2f);
        [Tooltip("Tốc độ cập nhật thay đổi cao độ tiếng bíp.")]
        public float ReaderBeepSpeed = 15f;

        [Header("Animations")]
        [Tooltip("Tên trạng thái hoạt ảnh khi lấy máy đo ra.")]
        public string EMFDrawState = "EMFDraw";
        [Tooltip("Tên trạng thái hoạt ảnh khi cất máy đo đi.")]
        public string EMFHideState = "EMFHide";
        [Tooltip("Tham số Trigger gọi hoạt ảnh cất máy đo.")]
        public string EMFHideTrigger = "Hide";

        [Header("Debug")]
        [Tooltip("Hiển thị bán kính quét (Gizmos) trong Editor.")]
        public bool ShowRadiusDebug;

        private float targetPitch;
        private float targetMilligauss;
        private float milligaussDecimal;

        private bool isEquipped;
        private bool isBusy;

        public override string Name => "EMF Meter";
        public override bool IsBusy() => !isEquipped || isBusy;
        public override bool CanCombine() => isEquipped && !isBusy;

        public override void OnUpdate()
        {
            if (!isEquipped || isBusy)
                return;

            var colliders = Physics.OverlapSphere(PlayerRoot.position, DetectionRadius, DetectionMask);
            if(colliders != null && colliders.Length > 0)
            {
                EMFAnomaly[] anomalies = (from col in colliders
                                          let anomaly = col.GetComponent<EMFAnomaly>()
                                          where anomaly != null
                                          select anomaly).ToArray();

                float[] milligausses = new float[anomalies.Length];
                for(int i = 0; i < anomalies.Length; i++)
                {
                    var anomaly = anomalies[i];
                    float anomalyMg = anomaly.GetMilligauss();
                    float minDistance = anomaly.MinDistance;

                    Vector3 playerPos = CameraRay.origin;
                    Vector3 anomalyPos = anomaly.transform.position;

                    float distance = Vector3.Distance(playerPos, anomalyPos);
                    float range = Mathf.InverseLerp(DetectionRadius, minDistance, distance);

                    if (range > anomaly.TimerStartRange)
                        anomaly.StartTimer();

                    Vector3 direction = (anomalyPos - playerPos).normalized;
                    float dot = Vector3.Dot(direction, LookForward);
                    dot = Mathf.Clamp(dot, MinAnomalyDirection, 1);

                    float compensatedDot = Mathf.Lerp(dot, 1, AnomalyDotRangeCompensation * range);
                    milligausses[i] = anomalyMg * range * compensatedDot;
                }

                float finalMilligauss = milligausses.Length > 0 ? milligausses.Max() : 0;
                targetMilligauss = Mathf.Lerp(targetMilligauss, finalMilligauss, Time.deltaTime * MilligaussUpdateSpeed);
            }

            // calculate background noise
            float backgroundNoise = BackgroundNoise;
            backgroundNoise += Mathf.PerlinNoise1D(Time.time * NoiseSpeed) * NoiseAmount;

            // select the largest milligauss 
            float milligauss = Mathf.Max(backgroundNoise, targetMilligauss);
            SetIndicators(milligauss);

            if (EneableReaderBeep && EneablePitchedBeep && ReaderAudio.isPlaying)
                ReaderAudio.pitch = Mathf.Lerp(ReaderAudio.pitch, targetPitch, Time.deltaTime * ReaderBeepSpeed);
        }

        private void SetIndicators(float milligauss)
        {
            DisplayMilligauss(milligauss);

            float level = Mathf.InverseLerp(0f, MaxMilligaussValue, milligauss);
            int indicatorsToEnable = Mathf.RoundToInt(level * Indicators.Count);

            bool playSound = false;
            float pitch = 1f;

            for (int i = 0; i < Indicators.Count; i++)
            {
                Material indicator = Indicators[i].material;

                if (i < indicatorsToEnable)
                {
                    indicator.EnableKeyword(EmissionKeyword);

                    if(i >= ReaderStartLevel)
                    {
                        float pitchT = Mathf.InverseLerp(ReaderStartLevel, Indicators.Count, i);
                        pitch = Mathf.Lerp(ReaderPitchLimits.RealMin, ReaderPitchLimits.RealMax, pitchT);
                        playSound = true;
                    }
                }
                else
                {
                    indicator.DisableKeyword(EmissionKeyword);
                }
            }

            if (EneableReaderBeep)
            {
                if (playSound)
                {
                    if (!ReaderAudio.isPlaying)
                        ReaderAudio.Play();

                    targetPitch = pitch;
                }
                else if (ReaderAudio.isPlaying)
                {
                    ReaderAudio.Stop();
                    ReaderAudio.pitch = 1f;
                    targetPitch = 1f;
                }
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

        private void DisplayMilligauss(float milligauss)
        {
            double roundedNumber = Math.Round(milligauss, 1);
            string mgText = roundedNumber.ToString("0.0", CultureInfo.InvariantCulture);

            if (mgText.Contains('.'))
            {
                string[] parts = mgText.Split('.');
                string wholePart = parts[0].TrimStart('0');

                if (float.TryParse(parts[1], out float secondPart))
                {
                    milligaussDecimal = Mathf.Lerp(milligaussDecimal, secondPart, Time.deltaTime * DecimalPartUpdateSpeed);

                    if (string.IsNullOrEmpty(wholePart))
                        wholePart = "0";

                    int decimalPart = Mathf.RoundToInt(milligaussDecimal);
                    string final = string.Format(DisplayFormat, wholePart, decimalPart);
                    MilligaussText.text = final;
                }
            }
            else
            {
                string final = string.Format(DisplayFormat, 0, 0);
                MilligaussText.text = final;
            }
        }

        public override void OnItemSelect()
        {
            ItemObject.SetActive(true);
            StartCoroutine(ShowEMF());
        }

        IEnumerator ShowEMF()
        {
            yield return new WaitForAnimatorClip(Animator, EMFDrawState, 0.5f);

            SetDisplay(true);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            StopAllCoroutines();
            StartCoroutine(HideEMF());

            isBusy = true;
            SetDisplay(false);
            SetIndicators(0f);

            if (ReaderAudio.isPlaying)
            {
                ReaderAudio.Stop();
                ReaderAudio.pitch = 1f;
                targetPitch = 1f;
            }

            targetPitch = 0f;
            targetMilligauss = 0f;
            Animator.SetTrigger(EMFHideTrigger);
        }

        IEnumerator HideEMF()
        {
            yield return new WaitForAnimatorClip(Animator, EMFHideState);

            ItemObject.SetActive(false);
            isEquipped = false;
            isBusy = false;
        }

        public override void OnItemActivate()
        {
            ItemObject.SetActive(true);
            Animator.Play(EMFDrawState, 0, 1f);
            SetDisplay(true);

            isEquipped = true;
            isBusy = false;
        }

        public override void OnItemDeactivate()
        {
            ItemObject.SetActive(false);
            SetDisplay(false);
            SetIndicators(0f);

            isEquipped = false;
            isBusy = false;
        }

        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (ShowRadiusDebug)
            {
                Gizmos.color = Color.magenta.Alpha(0.5f);
                Gizmos.DrawWireSphere(PlayerRoot.position, DetectionRadius);
            }
        }
    }
}