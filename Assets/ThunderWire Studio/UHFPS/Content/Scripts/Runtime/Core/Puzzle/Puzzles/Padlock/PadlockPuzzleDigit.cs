using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Padlock Puzzle Digit")]
    public class PadlockPuzzleDigit : MonoBehaviour, IInteractStart
    {
        public Axis RotateAxis;
        public float RotateSmoothTime = 0.3f;
        public int CurrentNumber = 0;
        public bool Inverse;

        [Header("Sounds")]
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