using System.Collections;
using UnityEngine;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class LeversPuzzleLever : MonoBehaviour, IInteractStart
    {
        private LeversPuzzle leversPuzzle;

        public Transform Target;
        public Transform LimitsObject;
        public MinMax SwitchLimits;
        public Axis LimitsForward = Axis.Z;
        public Axis LimitsNormal = Axis.Y;

        public SoundClip LeverOnSound;
        public SoundClip LeverOffSound;

        public Light LeverLight;
        public RendererMaterial LightRenderer;
        public string EmissionKeyword = "_EMISSION";

        public bool UseLight;
        public bool LeverState;

        private AudioSource audioSource;

        private float currentAngle;
        private bool canInteract = true;
        private bool canUse = true;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            leversPuzzle = GetComponentInParent<LeversPuzzle>();
            currentAngle = LeverState ? SwitchLimits.RealMax : SwitchLimits.RealMin;
            canInteract = true;
            canUse = true;
        }

        public void InteractStart()
        {
            if (!canInteract || !canUse) return;
            LeverState = !LeverState;

            bool leverState = leversPuzzle.LeversPuzzleType == LeversPuzzle.PuzzleType.LeversOrder || LeverState;
            OnLeverState(leverState);

            if(leversPuzzle.LeversPuzzleType == LeversPuzzle.PuzzleType.LeversChain)
                leversPuzzle.OnLeverInteract(this);

            canUse = false;
        }

        public void SetInteractState(bool state)
        {
            canInteract = state;
        }

        public void ResetLever()
        {
            if (LeverState)
            {
                StopAllCoroutines();
                StartCoroutine(DoLeverState(false, false));
            }

            LeverState = false;
        }

        public void ChangeLeverState()
        {
            if (!canInteract || !canUse) 
                return;

            LeverState = !LeverState;
            StopAllCoroutines();
            OnLeverState(LeverState);
            canUse = false;
        }

        public void SetLeverState(bool state)
        {
            currentAngle = state ? SwitchLimits.RealMax : SwitchLimits.RealMin;
            Vector3 axis = Quaternion.AngleAxis(currentAngle, LimitsObject.Direction(LimitsNormal)) * LimitsObject.Direction(LimitsForward);
            Target.rotation = Quaternion.LookRotation(axis);
            LeverState = state;
        }

        private void OnLeverState(bool state)
        {
            if (leversPuzzle.LeversPuzzleType != LeversPuzzle.PuzzleType.LeversOrder)
                StartCoroutine(DoLeverState(state, true));
            else
                StartCoroutine(LeverOrderPress());
        }

        IEnumerator DoLeverState(bool state, bool sendInteractEvent)
        {
            canUse = false;

            yield return SwitchLever(state ? SwitchLimits.RealMax : SwitchLimits.RealMin);

            if(sendInteractEvent && leversPuzzle.LeversPuzzleType != LeversPuzzle.PuzzleType.LeversChain) 
                leversPuzzle.OnLeverInteract(this);

            if (audioSource != null)
            {
                if (state) audioSource.PlayOneShotSoundClip(LeverOnSound);
                else audioSource.PlayOneShotSoundClip(LeverOffSound);
            }

            if (leversPuzzle.LeversPuzzleType != LeversPuzzle.PuzzleType.LeversOrder)
            {
                if (UseLight)
                {
                    if (state) LightRenderer.ClonedMaterial.EnableKeyword(EmissionKeyword);
                    else LightRenderer.ClonedMaterial.DisableKeyword(EmissionKeyword);
                    LeverLight.enabled = state;
                }
            }

            canUse = true;
        }

        IEnumerator SwitchLever(float targetAngle)
        {
            while (!Mathf.Approximately(currentAngle, targetAngle))
            {
                currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, Time.deltaTime * leversPuzzle.LeverSwitchSpeed * 100);
                Vector3 axis = Quaternion.AngleAxis(currentAngle, LimitsObject.Direction(LimitsNormal)) * LimitsObject.Direction(LimitsForward);
                Target.rotation = Quaternion.LookRotation(axis);
                yield return null;
            }
        }

        IEnumerator LeverOrderPress() 
        {
            yield return DoLeverState(true, true);
            yield return DoLeverState(false, false);
            LeverState = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (LimitsObject == null)
                return;

            Vector3 forward = LimitsObject.Direction(LimitsForward);
            Vector3 upward = LimitsObject.Direction(LimitsNormal);
            HandlesDrawing.DrawLimits(LimitsObject.position, SwitchLimits, forward, upward, true, radius: 0.25f);
        }
    }
}