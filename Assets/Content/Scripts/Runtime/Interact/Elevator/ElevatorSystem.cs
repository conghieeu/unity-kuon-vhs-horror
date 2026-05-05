using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using UHFPS.Input;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Hệ thống thang máy cốt lõi, quản lý các tầng, di chuyển, đóng/mở cửa và âm thanh thang máy.")]
    public class ElevatorSystem : MonoBehaviour, ISaveable
    {
        public enum ElevatorState
        {
            Idle,
            Moving,
            DoorOpening,
            DoorOpen,
            DoorClosing
        }

        public Animator Animator;
        public AudioSource AudioSource;

        [Tooltip("Danh sách điểm Transform tương ứng với vị trí các Tầng.")]
        public List<Transform> Floors = new();

        [Tooltip("Khoảng cách bù trừ dùng cho Gizmos hiển thị Tầng trong Editor.")]
        public Vector3 FloorOffset;

        [Tooltip("Thời gian (giây) để thang máy đi qua 1 tầng.")]
        public float OneFloorDuration = 10f;

        [Tooltip("Thời gian (giây) tự động đóng cửa sau khi mở.")]
        public float AutoDoorCloseTime = 5f;

        [Tooltip("Chỉ di chuyển theo trục dọc (Y), giữ nguyên X/Z.")]
        public bool VerticalMoveOnly;

        [Tooltip("Tham số Trigger Animator để mở cửa.")]
        public string OpenDoorTrigger = "Open";

        [Tooltip("Tham số Trigger Animator để đóng cửa.")]
        public string CloseDoorTrigger = "Close";

        [Tooltip("Tên State Animator khi cửa đang mở hoàn toàn.")]
        public string OpenDoorState = "DoorOpen";

        [Tooltip("Tên State Animator khi cửa đang đóng hoàn toàn.")]
        public string CloseDoorState = "DoorClose";

        [Tooltip("Âm thanh khi thang máy bắt đầu di chuyển.")]
        public SoundClip ElevatorStartMove;

        [Tooltip("Âm thanh khi thang máy tới đích.")]
        public SoundClip ElevatorEnd;
        [Space]

        [Tooltip("Âm thanh mở cửa.")]
        public SoundClip ElevatorOpenClean;

        [Tooltip("Âm thanh tiếng bíp khi mở cửa.")]
        public SoundClip ElevatorOpenBeep;

        [Tooltip("Âm thanh đóng cửa.")]
        public SoundClip ElevatorClose;

        [Tooltip("Sự kiện gọi ra khi người chơi bước vào trong thang máy.")]
        public UnityEvent OnElevatorEnter;

        [Tooltip("Sự kiện gọi ra khi người chơi bước ra ngoài.")]
        public UnityEvent OnElevatorExit;

        [Tooltip("Sự kiện gọi ra khi thang máy tới tầng mục tiêu.")]
        public UnityEvent OnElevatorEndMove;

        [Tooltip("Sự kiện gọi ra khi thang máy bắt đầu di chuyển, trả về số tầng mục tiêu.")]
        public UnityEvent<int> OnElevatorStartMove;

        public ElevatorState State => currentState;
        public bool PlayerEntered => isEntered;

        private ElevatorState currentState = ElevatorState.Idle;
        private ElevatorInteract elevatorCall;

        private int currentFloor;
        private bool isEntered;

        private void Start()
        {
            float distance = Mathf.Infinity;
            for (int i = 0; i < Floors.Count; i++)
            {
                float currDistance = Vector3.Distance(transform.position, Floors[i].position);
                if (currDistance < distance)
                {
                    distance = currDistance;
                    currentFloor = i;
                }
            }
        }

        public void OnElevatorTriggerEnter(bool enter)
        {
            isEntered = enter;
            StopAllCoroutines();

            if (enter)
            {
                if (elevatorCall != null && elevatorCall.InteractType == ElevatorInteract.InteractTypeEnum.CallElevator)
                {
                    elevatorCall.SetEmission(false);
                    elevatorCall = null;
                }

                InputManager.ResetToggledButtons();
                OnElevatorEnter?.Invoke();
            }
            else
            {
                OnElevatorExit?.Invoke();
                StartCoroutine(OnAutoCloseDoor());
            }

            if (currentState == ElevatorState.DoorClosing)
            {
                AudioSource.PlayOneShotSoundClip(ElevatorOpenClean);
                Animator.CrossFade(OpenDoorState, 1f);
            }

            currentState = ElevatorState.DoorOpen;
        }

        public bool CallElevator(ElevatorInteract call)
        {
            int level = (int)call.FloorLevel;
            if (currentState != ElevatorState.Idle)
                return false;

            if (elevatorCall != null)
                elevatorCall.SetEmission(false);
            elevatorCall = call;

            if (currentFloor == level && currentState != ElevatorState.DoorOpen)
            {
                AudioSource.PlayOneShotSoundClip(ElevatorOpenClean);
                StartCoroutine(OpenDoor());
            }
            else if (currentFloor != level)
            {
                StartCoroutine(MoveElevator(level));
            }

            return true;
        }

        public void MoveElevatorToLevel(ElevatorInteract call)
        {
            int level = (int)call.FloorLevel;
            if (currentState != ElevatorState.DoorOpen || currentFloor == level)
                return;

            if (elevatorCall != null) 
                elevatorCall.SetEmission(false);
            elevatorCall = call;

            StartCoroutine(MoveElevator(level));
        }

        IEnumerator OpenDoor()
        {
            Animator.SetTrigger(OpenDoorTrigger);
            currentState = ElevatorState.DoorOpening;

            yield return new WaitForAnimatorStateEnd(Animator, OpenDoorState);
            currentState = ElevatorState.DoorOpen;

            if (elevatorCall != null)
            {
                elevatorCall.SetEmission(false);
                elevatorCall = null;
            }
        }

        IEnumerator OnAutoCloseDoor()
        {
            if (currentState == ElevatorState.DoorOpen || currentState == ElevatorState.DoorOpening)
            {
                yield return new WaitForSeconds(AutoDoorCloseTime);

                Animator.SetTrigger(CloseDoorTrigger);
                AudioSource.PlayOneShotSoundClip(ElevatorClose);

                currentState = ElevatorState.DoorClosing;
                yield return new WaitForAnimatorStateEnd(Animator, CloseDoorState);

                currentState = ElevatorState.Idle;
            }
        }

        IEnumerator MoveElevator(int targetFloor)
        {
            if (currentState == ElevatorState.DoorOpen)
            {
                Animator.SetTrigger(CloseDoorTrigger);
                AudioSource.PlayOneShotSoundClip(ElevatorClose);

                currentState = ElevatorState.DoorClosing;
                yield return new WaitForAnimatorStateEnd(Animator, CloseDoorState);
                yield return new WaitUntil(() => !AudioSource.isPlaying);
            }

            AudioSource.SetSoundClip(ElevatorStartMove, 1, true);

            OnElevatorStartMove?.Invoke(targetFloor);
            currentState = ElevatorState.Moving;

            Vector3 startPos = transform.position;
            Vector3 endPos = Floors[targetFloor].position;
            if (VerticalMoveOnly)
            {
                endPos.x = startPos.x;
                endPos.z = startPos.z;
            }

            int floorDifference = Mathf.Abs(currentFloor - targetFloor);
            float moveDuration = OneFloorDuration * floorDifference;
            float elapsed = 0;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = GameTools.SmootherStep(0f, 1f, elapsed / moveDuration);
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;
            currentFloor = targetFloor;

            AudioSource.Stop();
            AudioSource.PlayOneShotSoundClip(ElevatorEnd);
            yield return new WaitForSeconds(ElevatorEnd.audioClip.length);

            AudioSource.PlayOneShotSoundClip(ElevatorOpenBeep);
            OnElevatorEndMove?.Invoke();

            yield return OpenDoor();
        }

        public void OnEnterElevator()
        {
            OnElevatorEnter?.Invoke();
        }

        public void OnExitElevator()
        {
            OnElevatorExit?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (Floors.Count == 0) return;

            Gizmos.color = Color.red.Alpha(0.5f);
            for (int i = 0; i < Floors.Count; i++)
            {
                Vector3 position = Floors[i].position + FloorOffset;
                Gizmos.DrawCube(position, Vector3.one * 0.1f);
                GizmosE.DrawCenteredLabel(position, $"Floor {i}");
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection() { { "floor", currentFloor } };
        }

        public void OnLoad(JToken data)
        {
            currentFloor = (int)data["floor"];
            MoveToFloorInstantly(currentFloor);
        }

        private void MoveToFloorInstantly(int floor)
        {
            Vector3 floorPos = Floors[floor].position;
            Vector3 elevatorPos = transform.position;

            if (VerticalMoveOnly)
                elevatorPos.y = floorPos.y;
            else
                elevatorPos = floorPos;

            transform.position = elevatorPos;
        }
    }
}