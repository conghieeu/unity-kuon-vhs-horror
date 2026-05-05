using System.Collections;
using UnityEngine;
using UHFPS.Tools;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    [Summary("Thành phần điều khiển vật lý và logic của một Đòn bẩy con trong hệ thống Levers Puzzle.")]
    public class LeversPuzzleLever : MonoBehaviour, IInteractStart
    {
        private LeversPuzzle leversPuzzle;

        [Tooltip("Mô hình Transform của đòn bẩy sẽ bị xoay khi người chơi gạt.")]
        public Transform Target;

        [Tooltip("Transform gốc đóng vai trò làm tâm xoay và giới hạn xoay.")]
        public Transform LimitsObject;

        [Tooltip("Giới hạn góc xoay tối thiểu và tối đa của đòn bẩy.")]
        public MinMax SwitchLimits;

        [Tooltip("Trục hướng tới của đòn bẩy (Thường là Z).")]
        public Axis LimitsForward = Axis.Z;

        [Tooltip("Trục pháp tuyến của đòn bẩy (Thường là Y).")]
        public Axis LimitsNormal = Axis.Y;

        [Tooltip("Âm thanh khi gạt đòn bẩy lên (Bật).")]
        public SoundClip LeverOnSound;

        [Tooltip("Âm thanh khi gạt đòn bẩy xuống (Tắt).")]
        public SoundClip LeverOffSound;

        [Tooltip("Đèn báo hiệu tích hợp ngay trên đòn bẩy.")]
        public Light LeverLight;

        [Tooltip("Mesh Renderer của đèn để đổi màu Emission.")]
        public RendererMaterial LightRenderer;
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Sử dụng đèn báo hiệu tích hợp trên đòn bẩy này.")]
        public bool UseLight;

        [Tooltip("Trạng thái Bật/Tắt hiện tại của đòn bẩy (Có thể đánh dấu để gạt mặc định lúc đầu).")]
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