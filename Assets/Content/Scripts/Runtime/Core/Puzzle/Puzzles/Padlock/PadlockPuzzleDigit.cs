using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Padlock Puzzle Digit")]
    [Summary("Thành phần gán vào các vòng xoay số của Padlock Puzzle. Quản lý việc xoay và chuyển số (0-9).")]
    public class PadlockPuzzleDigit : MonoBehaviour, IInteractStart
    {
        [Tooltip("Trục xoay của vòng số.")]
        public Axis RotateAxis;

        [Tooltip("Độ mượt (thời gian) khi vòng số xoay sang số tiếp theo.")]
        public float RotateSmoothTime = 0.3f;

        [Tooltip("Số hiện tại đang hiển thị (0-9).")]
        public int CurrentNumber = 0;

        [Tooltip("Đảo ngược chiều xoay của vòng số.")]
        public bool Inverse;

        [Header("Sounds")]
        [Tooltip("Âm thanh phát ra mỗi khi vòng số xoay được một nấc.")]
        public SoundClip TurnSound;

        private PadlockPuzzle padlockPuzzle;
        private Vector3 currRotation;
        private float nextRotation;
        private float velocity;

        private void Awake()
        {
            padlockPuzzle = transform.GetComponentInParent<PadlockPuzzle>();
            currRotation = transform.localEulerAngles;
            nextRotation = CurrentNumber * (360f / 10);
        }

        public void InteractStart()
        {
            if (!padlockPuzzle || padlockPuzzle.isUnlocked) 
                return;

            CurrentNumber = (CurrentNumber + 1) % 10;
            padlockPuzzle.SetDigit(this, CurrentNumber);
            nextRotation = CurrentNumber * (360f / 10);
            nextRotation *= Inverse ? -1 : 1;

            GameTools.PlayOneShot3D(transform.position, TurnSound);
        }

        private void Update()
        {
            float currAngle = currRotation.Component(RotateAxis);
            currAngle = Mathf.SmoothDampAngle(currAngle, nextRotation, ref velocity, RotateSmoothTime);
            currRotation = currRotation.SetComponent(RotateAxis, currAngle);
            transform.localEulerAngles = currRotation;
        }
    }
}