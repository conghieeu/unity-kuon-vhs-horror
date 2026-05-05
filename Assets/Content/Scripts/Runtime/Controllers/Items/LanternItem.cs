using System.Collections;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Điều khiển vật phẩm Đèn lồng (Lantern), cung cấp nguồn sáng với hiệu ứng nhấp nháy, vung vẩy và quản lý nhiên liệu.")]
    public class LanternItem : PlayerItemBehaviour
    {
        [System.Serializable]
        public struct HandleVariationStruct
        {
            public MinMax HandleVariation;
            public float HandleVariationSpeed;
        }

        [Header("Lantern Settings")]
        [Tooltip("Vật phẩm Nhiên liệu trong Inventory dùng để nạp lại đèn lồng.")]
        public ItemGuid FuelInventoryItem;
        [Tooltip("Transform gốc của tay cầm đèn lồng.")]
        public Transform HandleBone;
        [Tooltip("Nguồn sáng chính của đèn lồng.")]
        public Light LanternLight;
        [Tooltip("Renderer hiển thị ngọn lửa bên trong đèn lồng.")]
        public MeshRenderer LanternFlame;
        [Tooltip("Giới hạn góc độ xoay của tay cầm (Min-Max).")]
        public MinMax HandleLimits;
        [Tooltip("Trục xoay của tay cầm.")]
        public Axis HandleAxis;

        [Tooltip("Thời gian để tay cầm xoay theo trọng lực.")]
        public float HandleGravityTime = 0.2f;
        [Tooltip("Góc xoay hướng về phía trước của tay cầm.")]
        public float HandleForwardAngle = -90f;
        [Tooltip("Tốc độ thay đổi cường độ sáng của ngọn lửa.")]
        public float FlameChangeSpeed = 1f;
        [Tooltip("Cường độ sáng cơ bản của ngọn lửa.")]
        public float FlameLightIntensity = 1f;
        [Tooltip("Mức nhiên liệu bắt đầu làm mờ (Alpha fade) ngọn lửa.")]
        public float FlameAlphaFadeStart = 0.2f;

        [Tooltip("Giới hạn độ nhấp nháy của ngọn lửa (Min-Max).")]
        public MinMax FlameFlickerLimits;
        [Tooltip("Tốc độ nhấp nháy của ngọn lửa.")]
        public float FlameFlickerSpeed;

        [Header("Handle Variation")]
        [Tooltip("Độ dao động của tay cầm khi đứng yên.")]
        public HandleVariationStruct HandleIdleVariation;
        [Tooltip("Độ dao động của tay cầm khi đi bộ.")]
        public HandleVariationStruct HandleWalkVariation;
        [Tooltip("Thời gian hòa trộn giữa các trạng thái dao động.")]
        public float VariationBlendTime;
        [Tooltip("Sử dụng dao động tay cầm hay không?")]
        public bool UseHandleVariation;

        [Header("Fuel Settings")]
        [Tooltip("Kích hoạt nếu muốn đèn lồng không bao giờ cạn nhiên liệu.")]
        public bool InfiniteFuel = false;
        [Tooltip("Thời gian nạp lại nhiên liệu (tính bằng giây).")]
        public float FuelReloadTime = 2f;
        [Tooltip("Thời lượng nhiên liệu tối đa (tính bằng giây).")]
        public ushort FuelLife = 320;
        [Tooltip("Tỉ lệ phần trăm nhiên liệu ban đầu.")]
        public Percentage FuelPercentage = 100;

        [Header("Animations")]
        [Tooltip("Trạng thái hoạt ảnh khi lấy đèn lồng ra.")]
        public string LanternDrawState = "LanternDraw";
        [Tooltip("Trạng thái hoạt ảnh khi cất đèn lồng đi.")]
        public string LanternHideState = "LanternHide";
        [Tooltip("Trạng thái hoạt ảnh bắt đầu nạp nhiên liệu.")]
        public string LanternReloadStartState = "Lantern_Reload_Start";
        [Tooltip("Trạng thái hoạt ảnh kết thúc nạp nhiên liệu.")]
        public string LanternReloadEndState = "Lantern_Reload_End";
        [Tooltip("Trạng thái hoạt ảnh khi đèn lồng ở trạng thái nghỉ.")]
        public string LanternIdleState = "LanternIdle";

        [Header("Triggers")]
        [Tooltip("Tham số Trigger gọi hoạt ảnh cất đèn lồng.")]
        public string LanternHideTrigger = "Hide";
        [Tooltip("Tham số Trigger gọi hoạt ảnh nạp nhiên liệu.")]
        public string LanternReloadTrigger = "Reload";
        [Tooltip("Tham số Trigger gọi hoạt ảnh kết thúc nạp nhiên liệu.")]
        public string LanternReloadEndTrigger = "ReloadEnd";

        [Header("Sounds")]
        [Tooltip("Âm thanh khi lấy đèn lồng ra.")]
        public SoundClip LanternDraw;
        [Tooltip("Âm thanh khi cất đèn lồng đi.")]
        public SoundClip LanternHide;
        [Tooltip("Âm thanh khi nạp nhiên liệu.")]
        public SoundClip LanternReload;

        private AudioSource audioSource;
        private CanvasGroup lanternPanel;
        private CanvasGroup lanternFlame;

        private bool updateHandle;
        private float handleAngle;
        private float handleVelocity;

        private float flameLerp;
        private float targetFlame;
        private float flameIntensity;
        private float variationBlend;
        private float variationVelocity;

        public float lanternFuel;
        private float currentFuel;

        private bool isEquipped;
        private bool isBusy;

        public override string Name => "Lantern";

        public override bool IsBusy() => !isEquipped || isBusy;

        public override bool CanCombine() => isEquipped && !isBusy;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            GameManager gameManager = GameManager.Instance;

            var behaviours = gameManager.GraphicReferences.Value["Lantern"];
            lanternPanel = (CanvasGroup)behaviours[0];
            lanternFlame = (CanvasGroup)behaviours[1];

            if (!SaveGameManager.GameWillLoad)
            {
                currentFuel = FuelPercentage.From(FuelLife);
                UpdateFuel();
            }
        }

        private void LateUpdate()
        {
            if (!updateHandle)
                return;

            float lookY = LookController.LookRotation.y;
            MinMax lookLimits = LookController.VerticalLimits;
            Vector3 movement = PlayerStateMachine.Controller.velocity;

            float lookInverse1 = Mathf.InverseLerp(lookLimits.min, 0, lookY);
            float lookInverse2 = Mathf.InverseLerp(0, lookLimits.max, lookY);

            float lerp1 = Mathf.Lerp(HandleLimits.min, HandleForwardAngle, lookInverse1);
            float lerp2 = Mathf.Lerp(HandleForwardAngle, HandleLimits.max, lookInverse2);

            float targetInverse = lookInverse1 + lookInverse2;
            float targetAngle = Mathf.Lerp(lerp1, lerp2, targetInverse);

            float movementVariation = 0f;
            if (UseHandleVariation)
            {
                float idleNoise = Mathf.PerlinNoise(Time.time * HandleIdleVariation.HandleVariationSpeed, 0);
                float idleVariation = Mathf.Lerp(HandleIdleVariation.HandleVariation.RealMin, HandleIdleVariation.HandleVariation.RealMax, idleNoise);

                float walkNoise = Mathf.PerlinNoise(Time.time * HandleWalkVariation.HandleVariationSpeed, 0);
                float walkVariation = Mathf.Lerp(HandleWalkVariation.HandleVariation.RealMin, HandleWalkVariation.HandleVariation.RealMax, walkNoise);

                movement.y = 0f;
                movement = Vector3.ClampMagnitude(movement, 1f);
                variationBlend = Mathf.SmoothDamp(variationBlend, movement.magnitude, ref variationVelocity, VariationBlendTime);
                movementVariation = Mathf.Lerp(idleVariation, walkVariation, variationBlend);
            }

            handleAngle = Mathf.SmoothDamp(handleAngle, targetAngle, ref handleVelocity, HandleGravityTime);
            HandleBone.localRotation = Quaternion.AngleAxis(handleAngle + movementVariation, HandleAxis.Convert());
        }

        public override void OnUpdate()
        {
            if (!updateHandle)
                return;

            float flicker = Mathf.PerlinNoise(Time.time * FlameFlickerSpeed, 0);
            flameIntensity = Mathf.Lerp(FlameFlickerLimits.RealMin, FlameFlickerLimits.RealMax, flicker) * FlameLightIntensity;

            if (isEquipped && !isBusy && !InfiniteFuel)
            {
                // lantern fuel
                currentFuel = currentFuel > 0 ? currentFuel -= Time.deltaTime : 0;
                UpdateFuel();
            }

            float fuelFlameIntensity = flameIntensity * lanternFuel;
            flameLerp = Mathf.MoveTowards(flameLerp, targetFlame, Time.deltaTime * FlameChangeSpeed);
            LanternLight.intensity = Mathf.Lerp(0f, fuelFlameIntensity, flameLerp);
        }

        private void UpdateFuel()
        {
            lanternFuel = Mathf.InverseLerp(0, FuelLife, currentFuel);
            lanternFlame.alpha = lanternFuel;

            if (LanternFlame != null)
            {
                float mappedT = Mathf.InverseLerp(0, FlameAlphaFadeStart, currentFuel);
                float flameAlpha = Mathf.Lerp(1, 0, 1 - mappedT);
                LanternFlame.material.SetFloat("_Fade", flameAlpha);
            }
        }

        public override void OnItemCombine(InventoryItem combineItem)
        {
            if (combineItem.ItemGuid != FuelInventoryItem || !isEquipped)
                return;

            Inventory.Instance.RemoveItem(combineItem, 1);
            Animator.SetTrigger(LanternReloadTrigger);
            StartCoroutine(ReloadLantern());
            isBusy = true;
        }

        IEnumerator ReloadLantern()
        {
            yield return new WaitForAnimatorClip(Animator, LanternReloadStartState);

            audioSource.PlayOneShotSoundClip(LanternReload);
            yield return new WaitForSeconds(FuelReloadTime);

            Animator.SetTrigger(LanternReloadEndTrigger);
            currentFuel = FuelPercentage.From(FuelLife);
            UpdateFuel();

            yield return new WaitForAnimatorClip(Animator, LanternReloadEndState);

            isBusy = false;
        }

        public override void OnItemSelect()
        {
            CanvasGroupFader.StartFadeInstance(lanternPanel, true, 5f);

            ItemObject.SetActive(true);
            StartCoroutine(SelectLantern());
            audioSource.PlayOneShotSoundClip(LanternDraw);

            flameLerp = 0f;
            targetFlame = 1f;
            updateHandle = true;
            isEquipped = false;
            isBusy = false;
        }

        IEnumerator SelectLantern()
        {
            yield return new WaitForAnimatorClip(Animator, LanternDrawState);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            CanvasGroupFader.StartFadeInstance(lanternPanel, false, 5f,
                () => lanternPanel.gameObject.SetActive(false));

            StopAllCoroutines();
            StartCoroutine(HideLantern());
            Animator.SetTrigger(LanternHideTrigger);
            audioSource.PlayOneShotSoundClip(LanternHide);

            targetFlame = 0f;
            isBusy = true;
        }

        IEnumerator HideLantern()
        {
            yield return new WaitForAnimatorClip(Animator, LanternHideState);
            yield return new WaitUntil(() => flameLerp <= 0f);
            ItemObject.SetActive(false);
            updateHandle = false;
            isEquipped = false;
            isBusy = false;
        }

        public override void OnItemActivate()
        {
            lanternPanel.alpha = 1f;
            lanternPanel.gameObject.SetActive(true);

            StopAllCoroutines();
            ItemObject.SetActive(true);
            Animator.Play(LanternIdleState);

            flameLerp = 1f;
            targetFlame = 1f;

            updateHandle = true;
            isEquipped = true;
            isBusy = false;
        }

        public override void OnItemDeactivate()
        {
            lanternPanel.alpha = 0f;
            lanternPanel.gameObject.SetActive(false);

            StopAllCoroutines();
            ItemObject.SetActive(false);
            updateHandle = false;
            isEquipped = false;
            isBusy = false;
        }

        public override StorableCollection OnCustomSave()
        {
            return new StorableCollection()
            {
                { "currentFuel", currentFuel }
            };
        }

        public override void OnCustomLoad(JToken data)
        {
            currentFuel = FuelPercentage.From(FuelLife);
            UpdateFuel();
        }
    }
}