using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class MazePuzzle : PuzzleBaseBlend, ISaveable
    {
        public enum TriggerType { PutBall, GrabBall, WrongHole, FinishHole }

        public Transform MazeTransform;

        public ItemGuid BallItem;
        public Rigidbody BallObject;
        public Transform BallStart;

        public Collider PutBallTrigger;
        public Collider GrabBallTrigger;
        public GameObject GrabBallAnim;

        public Animator MazeAnimator;
        public string OpenDrawerState = "Open";
        public string GrabBallState = "Grab";

        public Vector3 MazeLiftOffset;
        public Vector3 RotationOffset;

        public Vector3 GrabBallPosition;
        public Vector3 GrabBallRotation;
        public float GrabBallDuration;

        public LayerMask CullLayers;
        public Layer InteractLayer;

        public float LiftDuration;
        public float ReturnDuration;
        public float RotateSpeed;

        public Axis VerticalAxis = Axis.X;
        public Axis HorizontalAxis = Axis.Z;
        public MinMax VerticalLimits = new(-45, 45);
        public MinMax HorizontalLimits = new(-45, 45);

        public UnityEvent OnBallEnterWrongHole;
        public UnityEvent OnBallEnterFinishHole;

        private Inventory inventory;

        private Vector3 defaulPosition;
        private Quaternion defaulRotation;
        private Vector2 mazeRotation;

        private bool puzzleCompleted;
        private bool mazeRotateLocked;
        private bool isBallGrabbed;

        public override void Awake()
        {
            base.Awake();
            inventory = Inventory.Instance;
            defaulPosition = MazeTransform.position;
            defaulRotation = MazeTransform.rotation;
        }

        public override void Update()
        {
            base.Update();

            if (isActive && !mazeRotateLocked) 
            {
                if (InputManager.ReadButton(Controls.LEFT_BUTTON))
                {
                    Vector2 rotateValue = InputManager.ReadInput<Vector2>(Controls.LOOK) * RotateSpeed;
                    mazeRotation.y += rotateValue.y;
                    mazeRotation.x += -rotateValue.x;

                    mazeRotation.y = ClampAngle(mazeRotation.y, VerticalLimits.RealMin, VerticalLimits.RealMax);
                    mazeRotation.x = ClampAngle(mazeRotation.x, HorizontalLimits.RealMin, HorizontalLimits.RealMax);

                    Vector3 vertical = MazeTransform.Direction(VerticalAxis);
                    Vector3 horizontal = MazeTransform.Direction(HorizontalAxis);

                    Quaternion rotationY = Quaternion.AngleAxis(mazeRotation.y, vertical);
                    Quaternion rotationX = Quaternion.AngleAxis(mazeRotation.x, horizontal);
                    Quaternion offset = Quaternion.Euler(RotationOffset);

                    MazeTransform.rotation = offset * rotationY * rotationX;
                }
            }
        }

        public override void OnBlendedIn()
        {
            gameManager.ShowPointer(CullLayers, InteractLayer, (hit, interactStart) =>
            {
                interactStart.InteractStart();
            });

            PutBallTrigger.enabled = BallItem.InInventory;
        }

        public override void OnBlendStart(bool blendIn)
        {
            // move maze to start/end position or rotation
            StopAllCoroutines();
            StartCoroutine(LiftPuzzle(blendIn));

            if (!blendIn)
            {
                mazeRotation = Vector3.zero;
                PutBallTrigger.enabled = false;
                GrabBallTrigger.enabled = false;
                isBallGrabbed = false;
            }
            else return;

            gameManager.HidePointer();
            BallObject.linearVelocity = Vector3.zero;
            BallObject.gameObject.SetActive(false);
        }

        public void OnMazeTrigger(TriggerType trigger)
        {
            if(trigger == TriggerType.PutBall)
            {
                PutBallTrigger.enabled = false;
                BallObject.transform.position = BallStart.position;
                BallObject.linearVelocity = Vector3.zero;
                BallObject.gameObject.SetActive(true);
            }
            else if (trigger == TriggerType.GrabBall)
            {
                GrabBallAnim.SetActive(false);
                GrabBallTrigger.enabled = false;
                isBallGrabbed = true;
            }
            else if (trigger == TriggerType.WrongHole)
            {
                BallObject.gameObject.SetActive(false);
                StartCoroutine(OnGrabBallRotate());
                OnBallEnterWrongHole?.Invoke();
            }
            else if (trigger == TriggerType.FinishHole)
            {
                switchColliders = false;
                inventory.RemoveItem(BallItem);
                MazeAnimator.Play(OpenDrawerState);
                SwitchBack();

                OnBallEnterFinishHole?.Invoke();
                puzzleCompleted = true;
            }
        }

        IEnumerator LiftPuzzle(bool liftUp)
        {
            mazeRotateLocked = true;
            MazeTransform.GetPositionAndRotation(out Vector3 currPos, out Quaternion currRot);

            Vector3 endPos = liftUp 
                ? defaulPosition + MazeLiftOffset
                : defaulPosition;
            Quaternion endRot = liftUp 
                ? Quaternion.Euler(RotationOffset)
                : defaulRotation;

            float duration = liftUp ? LiftDuration : ReturnDuration;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = GameTools.SmootherStep(0f, 1f, elapsed / duration);
                MazeTransform.position = Vector3.Lerp(currPos, endPos, t);
                MazeTransform.rotation = Quaternion.Slerp(currRot, endRot, t);
                yield return null;
            }

            mazeRotateLocked = false;
        }

        IEnumerator OnGrabBallRotate()
        {
            mazeRotateLocked = true;
            canManuallySwitch = false;

            GrabBallAnim.SetActive(true);
            MazeAnimator.Play(GrabBallState);

            MazeTransform.GetPositionAndRotation(out Vector3 currPos, out Quaternion currRot);
            Vector3 endPos = currPos + GrabBallPosition;
            Quaternion endRot = Quaternion.Euler(GrabBallRotation);

            float elapsed = 0;
            while (elapsed < GrabBallDuration)
            {
                elapsed += Time.deltaTime;
                float t = GameTools.SmootherStep(0f, 1f, elapsed / GrabBallDuration);
                MazeTransform.position = Vector3.Lerp(currPos, endPos, t);
                MazeTransform.rotation = Quaternion.Slerp(currRot, endRot, t);
                yield return null;
            }

            GrabBallTrigger.enabled = true;
            yield return new WaitUntil(() => isBallGrabbed);

            Quaternion backRotation = Quaternion.Euler(RotationOffset);
            elapsed = 0;

            while (elapsed < GrabBallDuration)
            {
                elapsed += Time.deltaTime;
                float t = GameTools.SmootherStep(0f, 1f, elapsed / GrabBallDuration);
                MazeTransform.position = Vector3.Lerp(endPos, currPos, t);
                MazeTransform.rotation = Quaternion.Slerp(endRot, backRotation, t);
                yield return null;
            }

            MazeAnimator.Rebind();
            mazeRotation = Vector3.zero;
            PutBallTrigger.enabled = true;
            isBallGrabbed = false;

            canManuallySwitch = true;
            mazeRotateLocked = false;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            float newAngle = angle.FixAngle();
            return Mathf.Clamp(newAngle, min, max);
        }

        private void OnDrawGizmosSelected()
        {
            if (MazeTransform == null)
                return;

            Vector3 liftPosition = Application.isPlaying 
                ? defaulPosition + MazeLiftOffset
                : MazeTransform.position + MazeLiftOffset;

            if(BallStart != null)
            {
                Gizmos.color = Color.magenta.Alpha(0.5f);
                Gizmos.DrawSphere(BallStart.position, 0.01f);
            }

            Gizmos.color = Color.green.Alpha(0.5f);
            Gizmos.DrawSphere(liftPosition, 0.01f);

            Vector3 grabBallPosition = liftPosition + GrabBallPosition;

            Gizmos.color = Color.yellow.Alpha(0.5f);
            Gizmos.DrawSphere(grabBallPosition, 0.01f);
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(puzzleCompleted), puzzleCompleted },
            };
        }

        public void OnLoad(JToken data)
        {
            puzzleCompleted = (bool)data[nameof(puzzleCompleted)];

            if (puzzleCompleted)
            {
                PutBallTrigger.enabled = false;
                GrabBallTrigger.enabled = false;

                CollidersEnable.ForEach(x => x.enabled = true);
                CollidersDisable.ForEach(x => x.enabled = false);

                MazeAnimator.Play(OpenDrawerState, 0, 1f);
            }
        }
    }
}