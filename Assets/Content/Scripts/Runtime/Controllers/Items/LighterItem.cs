using System.Collections;
using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Điều khiển vật phẩm Bật lửa (Lighter), cho phép tạo nguồn sáng nhỏ với khả năng tắt lửa ngẫu nhiên.")]
    public class LighterItem : PlayerItemBehaviour
    {
        [Header("Lighter Settings")]
        [Tooltip("Nguồn sáng chính từ ngọn lửa bật lửa.")]
        public Light FlameLight;
        [Tooltip("Nguồn sáng tạm thời khi đánh tia lửa.")]
        public Light SparkLight;
        [Tooltip("Hệ thống hạt (Particle System) tạo hiệu ứng tia lửa.")]
        public ParticleSystem SparkParticle;
        [Tooltip("Renderer hiển thị ngọn lửa.")]
        public MeshRenderer FlameRenderer;

        [Tooltip("Thời gian hiển thị tia lửa (tính bằng giây).")]
        public float SparkLightTime;
        [Range(0f, 1f)]
        [Tooltip("Tỉ lệ phần trăm bật lửa thành công trong mỗi lần bật.")]
        public float FlameIgniteProbability = 0.5f;
        [Range(0f, 1f)]
        [Tooltip("Tỉ lệ phần trăm ngọn lửa bị tắt ngẫu nhiên.")]
        public float FlameExtinguishProbability = 0.5f;
        [Tooltip("Khoảng thời gian ngẫu nhiên (Min-Max) để ngọn lửa bị tắt.")]
        public MinMax FlameExtinguishTimeRange;
        [Tooltip("Kích hoạt tính năng ngọn lửa bị tắt ngẫu nhiên.")]
        public bool EnableFlameExtinguishing;

        [Tooltip("Giới hạn mức độ nhấp nháy của ngọn lửa (Min-Max).")]
        public MinMax FlameFlickerLimits;
        [Tooltip("Tốc độ nhấp nháy của ngọn lửa.")]
        public float FlameFlickerSpeed;
        [Tooltip("Cường độ sáng cơ bản của ngọn lửa.")]
        public float FlameLightIntensity = 1f;

        [Header("Animations")]
        [Tooltip("Trạng thái hoạt ảnh khi lấy bật lửa ra.")]
        public string LighterDrawState = "LighterDraw";
        [Tooltip("Trạng thái hoạt ảnh khi cất bật lửa đi.")]
        public string LighterHideState = "LighterHide";
        [Tooltip("Trạng thái hoạt ảnh khi bắt đầu bật lửa.")]
        public string LighterIgniteStartState = "LighterIgniteStart";
        [Tooltip("Trạng thái hoạt ảnh khi đánh tia lửa.")]
        public string LighterIgniteSparkState = "LighterIgniteSpark";
        [Tooltip("Trạng thái hoạt ảnh khi giữ ngọn lửa cháy.")]
        public string LighterIgniteHoldState = "LighterIgniteHold";

        [Header("Triggers")]
        [Tooltip("Tham số Trigger gọi hoạt ảnh cất bật lửa.")]
        public string LighterHideTrigger = "Hide";
        [Tooltip("Tham số Trigger gọi hoạt ảnh đánh tia lửa.")]
        public string LighterSparkTrigger = "Spark";
        [Tooltip("Tham số Boolean điều khiển việc giữ ngọn lửa cháy.")]
        public string LighterHoldTrigger = "Hold";

        [Header("Sounds")]
        [Tooltip("Âm thanh khi bật bật lửa (đánh tia lửa).")]
        public SoundClip LighterFlick;

        private AudioSource audioSource;
        private bool flameIgnited;
        private bool isEquipped;
        private bool isBusy;

        private float flameExtinguishTime;
        private float sparkTime;

        public override string Name => "Lighter";

        public override bool IsBusy() => !isEquipped || isBusy;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public override void OnUpdate()
        {
            if (isEquipped)
            {
                if (flameIgnited)
                {
                    float flicker = Mathf.PerlinNoise(Time.time * FlameFlickerSpeed, 0);
                    FlameLight.intensity = Mathf.Lerp(FlameFlickerLimits.RealMin, FlameFlickerLimits.RealMax, flicker) * FlameLightIntensity;

                    if (EnableFlameExtinguishing)
                    {
                        if ((flameExtinguishTime -= Time.deltaTime) <= 0)
                        {
                            if (PickTrueWithProbability(FlameExtinguishProbability))
                            {
                                FlameRenderer.gameObject.SetActive(false);
                                Animator.SetBool(LighterHoldTrigger, false);
                                StopAllCoroutines();
                                StartCoroutine(FlameExtinguished());
                                flameIgnited = false;
                            }

                            flameExtinguishTime = FlameExtinguishTimeRange.Random();
                        }
                    }
                }

                if (sparkTime > 0) sparkTime -= Time.deltaTime;
                else SparkLight.enabled = false;
            }
        }

        public override void OnItemSelect()
        {
            ItemObject.SetActive(true);
            StartCoroutine(ShowLighter());
            flameExtinguishTime = FlameExtinguishTimeRange.Random();
            isEquipped = false;
        }

        IEnumerator ShowLighter()
        {
            yield return new WaitForAnimatorClip(Animator, LighterDrawState);
            isEquipped = true;

            yield return IgniteLighter(true);
        }

        IEnumerator FlameExtinguished()
        {
            yield return new WaitForSeconds(1f);
            yield return IgniteLighter(false);
        }

        IEnumerator IgniteLighter(bool isStart)
        {
            bool isIgnited = PickTrueWithProbability(FlameIgniteProbability);

            if (isStart)
            {
                if (isIgnited)
                {
                    Animator.SetBool(LighterHoldTrigger, true);
                    Animator.SetBool(LighterSparkTrigger, false);
                }
                else Animator.SetBool(LighterSparkTrigger, true);
                yield return new WaitForAnimatorClip(Animator, LighterIgniteStartState);
            }
            else
            {
                Animator.SetBool(LighterSparkTrigger, true);
                if (isIgnited) Animator.SetBool(LighterHoldTrigger, true);
                yield return new WaitForAnimatorClip(Animator, LighterIgniteSparkState);
            }

            if (!isIgnited)
            {
                Animator.SetBool(LighterSparkTrigger, true);
                Animator.SetBool(LighterHoldTrigger, false);

                yield return new WaitForAnimatorStateEnter(Animator, LighterIgniteSparkState);

                do
                {
                    if (isIgnited = PickTrueWithProbability(FlameIgniteProbability))
                    {
                        Animator.SetBool(LighterSparkTrigger, false);
                        Animator.SetBool(LighterHoldTrigger, true);
                    }

                    yield return new WaitForAnimatorClip(Animator, LighterIgniteSparkState);
                }
                while (!isIgnited);
            }

            FlameRenderer.gameObject.SetActive(true);
            Animator.SetBool(LighterHoldTrigger, true);
            Animator.SetBool(LighterSparkTrigger, false);
            flameIgnited = true;
        }

        public void ShowSpark()
        {
            if (audioSource) audioSource.PlayOneShotSoundClip(LighterFlick);
            SparkParticle.Play();
            SparkLight.enabled = true;
            sparkTime = SparkLightTime;
        }

        public override void OnItemDeselect()
        {
            StopAllCoroutines();
            StartCoroutine(HideLighter());
            Animator.SetBool(LighterHoldTrigger, false);
            flameIgnited = false;
            isBusy = true;
        }

        IEnumerator HideLighter()
        {
            Animator.SetTrigger(LighterHideTrigger);
            FlameRenderer.gameObject.SetActive(false);
            SparkLight.enabled = true;

            yield return new WaitForAnimatorClip(Animator, LighterHideState);
            ItemObject.SetActive(false);

            sparkTime = 0;
            isEquipped = false;
            isBusy = false;
        }

        public override void OnItemActivate()
        {
            ItemObject.SetActive(true);
            Animator.Play(LighterIgniteHoldState);

            FlameRenderer.gameObject.SetActive(true);
            Animator.SetBool(LighterHoldTrigger, true);
            flameExtinguishTime = FlameExtinguishTimeRange.Random();

            StopAllCoroutines();
            flameIgnited = true;
            isEquipped = true;
        }

        public override void OnItemDeactivate()
        {
            StopAllCoroutines();

            FlameRenderer.gameObject.SetActive(true);
            SparkLight.enabled = true;
            ItemObject.SetActive(false);

            sparkTime = 0;
            flameIgnited = false;
            isEquipped = false;
            isBusy = false;
        }

        private bool PickTrueWithProbability(double probability)
        {
            System.Random rand = new();
            double randValue = rand.NextDouble();
            return randValue < probability;
        }
    }
}