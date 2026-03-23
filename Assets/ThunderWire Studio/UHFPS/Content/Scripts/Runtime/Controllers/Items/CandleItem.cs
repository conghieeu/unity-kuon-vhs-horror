using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class CandleItem : PlayerItemBehaviour
    {
        public Light FlameLight;
        public MeshRenderer FlameRenderer;

        public float NormalLightMultiplier = 1f;
        public float FocusLightMultiplier = 2f;

        public float FlameLightIntensity = 1f;
        public float FlameIntensityChangeSpeed = 1f;
        public MinMax FlameFlickerLimits;
        public float FlameFlickerSpeed;

        public string CandleDrawState = "CandleDraw";
        public string CandleHideState = "CandleHide";
        public string CandleIdleState = "CandleIdle";
        public string CandleFocusState = "CandleFocus";
        public string CandleUnfocusState = "CandleUnfocus";

        public string CandleFocusTrigger = "Focus";
        public string CandleBlowTrigger = "Blow";

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