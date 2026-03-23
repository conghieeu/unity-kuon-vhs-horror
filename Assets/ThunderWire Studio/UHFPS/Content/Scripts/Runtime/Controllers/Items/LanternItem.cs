using System.Collections;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class LanternItem : PlayerItemBehaviour
    {
        [System.Serializable]
        public struct HandleVariationStruct
        {
            public MinMax HandleVariation;
            public float HandleVariationSpeed;
        }

        public ItemGuid FuelInventoryItem;
        public Transform HandleBone;
        public Light LanternLight;
        public MeshRenderer LanternFlame;
        public MinMax HandleLimits;
        public Axis HandleAxis;

        public float HandleGravityTime = 0.2f;
        public float HandleForwardAngle = -90f;
        public float FlameChangeSpeed = 1f;
        public float FlameLightIntensity = 1f;
        public float FlameAlphaFadeStart = 0.2f;

        public MinMax FlameFlickerLimits;
        public float FlameFlickerSpeed;

        public HandleVariationStruct HandleIdleVariation;
        public HandleVariationStruct HandleWalkVariation;
        public float VariationBlendTime;
        public bool UseHandleVariation;

        public bool InfiniteFuel = false;
        public float FuelReloadTime = 2f;
        public ushort FuelLife = 320;
        public Percentage FuelPercentage = 100;

        public string LanternDrawState = "LanternDraw";
        public string LanternHideState = "LanternHide";
        public string LanternReloadStartState = "Lantern_Reload_Start";
        public string LanternReloadEndState = "Lantern_Reload_End";
        public string LanternIdleState = "LanternIdle";

        public string LanternHideTrigger = "Hide";
        public string LanternReloadTrigger = "Reload";
        public string LanternReloadEndTrigger = "ReloadEnd";

        public SoundClip LanternDraw;
        public SoundClip LanternHide;
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