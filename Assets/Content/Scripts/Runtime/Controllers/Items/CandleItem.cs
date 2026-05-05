using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Điều khiển vật phẩm Cây Nến, cung cấp nguồn sáng cơ bản với hiệu ứng nhấp nháy.")]
    public class CandleItem : PlayerItemBehaviour
    {
        [Header("Candle Settings")]
        [Tooltip("Nguồn sáng chính của cây nến.")]
        public Light FlameLight;
        [Tooltip("Lưới hiển thị (MeshRenderer) cho ngọn lửa của cây nến.")]
        public MeshRenderer FlameRenderer;

        [Tooltip("Hệ số nhân cường độ sáng bình thường.")]
        public float NormalLightMultiplier = 1f;
        [Tooltip("Hệ số nhân cường độ sáng khi người chơi đưa nến lại gần (Focus).")]
        public float FocusLightMultiplier = 2f;

        [Tooltip("Cường độ sáng cơ bản của ngọn lửa.")]
        public float FlameLightIntensity = 1f;
        [Tooltip("Tốc độ thay đổi cường độ sáng (ví dụ khi đưa nến lại gần).")]
        public float FlameIntensityChangeSpeed = 1f;
        [Tooltip("Giới hạn mức độ nhấp nháy của ngọn lửa (Min-Max).")]
        public MinMax FlameFlickerLimits;
        [Tooltip("Tốc độ nhấp nháy của ngọn lửa.")]
        public float FlameFlickerSpeed;

        [Header("Animations")]
        [Tooltip("Trạng thái hoạt ảnh khi lấy nến ra.")]
        public string CandleDrawState = "CandleDraw";
        [Tooltip("Trạng thái hoạt ảnh khi cất nến đi.")]
        public string CandleHideState = "CandleHide";
        [Tooltip("Trạng thái hoạt ảnh khi nến đang cầm trên tay (nghỉ).")]
        public string CandleIdleState = "CandleIdle";
        [Tooltip("Trạng thái hoạt ảnh khi đưa nến lại gần (Focus).")]
        public string CandleFocusState = "CandleFocus";
        [Tooltip("Trạng thái hoạt ảnh khi ngừng đưa nến lại gần (Unfocus).")]
        public string CandleUnfocusState = "CandleUnfocus";

        [Header("Triggers")]
        [Tooltip("Tham số Boolean điều khiển việc đưa nến lại gần (Focus).")]
        public string CandleFocusTrigger = "Focus";
        [Tooltip("Tham số Trigger điều khiển việc thổi tắt nến.")]
        public string CandleBlowTrigger = "Blow";

        [Header("Sounds")]
        [Tooltip("Âm thanh khi thổi tắt nến.")]
        public SoundClip FlameBlow;

        private AudioSource audioSource;
        private float newIntensity;
        private bool isEquipped;
        private bool isBusy;

        public override string Name => "Candle";

        public override bool IsBusy() => !isEquipped || isBusy;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            newIntensity = FlameLightIntensity;
        }

        public override void OnUpdate()
        {
            if (isEquipped)
            {
                float intensityMultiplier = NormalLightMultiplier;

                if (CanInteract && InputManager.ReadButton(Controls.ADS))
                {
                    Animator.SetBool(CandleFocusTrigger, true);
                    intensityMultiplier = FocusLightMultiplier;
                }
                else
                {
                    Animator.SetBool(CandleFocusTrigger, false);
                }

                float flicker = Mathf.PerlinNoise(Time.time * FlameFlickerSpeed, 0);
                newIntensity = Mathf.MoveTowards(newIntensity, FlameLightIntensity * intensityMultiplier, Time.deltaTime * FlameIntensityChangeSpeed);
                FlameLight.intensity = Mathf.Lerp(FlameFlickerLimits.RealMin, FlameFlickerLimits.RealMax, flicker) * newIntensity;
            }
        }

        public override void OnItemSelect()
        {
            ItemObject.SetActive(true);
            FlameRenderer.gameObject.SetActive(true);
            StartCoroutine(ShowCandle());
            isEquipped = false;
        }

        IEnumerator ShowCandle()
        {
            yield return new WaitForAnimatorClip(Animator, CandleDrawState);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            StopAllCoroutines();
            StartCoroutine(HideCandle());
            Animator.SetTrigger(CandleBlowTrigger);
            isBusy = true;
        }

        IEnumerator HideCandle()
        {
            yield return new WaitForAnimatorClip(Animator, CandleHideState);
            ItemObject.SetActive(false);
            isBusy = false;
            isEquipped = false;
        }

        public void BlowOutFlame()
        {
            FlameRenderer.gameObject.SetActive(false);
            audioSource.PlayOneShotSoundClip(FlameBlow);
        }

        public override void OnItemActivate()
        {
            ItemObject.SetActive(true);
            FlameRenderer.gameObject.SetActive(true);
            Animator.Play(CandleIdleState);

            StopAllCoroutines();
            isBusy = false;
            isEquipped = true;
        }

        public override void OnItemDeactivate()
        {
            StopAllCoroutines();
            FlameRenderer.gameObject.SetActive(true);
            ItemObject.SetActive(false);
            isEquipped = false;
            isBusy = false;
        }
    }
}