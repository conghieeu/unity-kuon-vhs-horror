using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using UHFPS.Input;

namespace UHFPS.Runtime
{
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

        public List<Transform> Floors = new();
        public Vector3 FloorOffset;
        public float OneFloorDuration = 10f;
        public float AutoDoorCloseTime = 5f;
        public bool VerticalMoveOnly;

        public string OpenDoorTrigger = "Open";
        public string CloseDoorTrigger = "Close";
        public string OpenDoorState = "DoorOpen";
        public string CloseDoorState = "DoorClose";

        public SoundClip ElevatorStartMove;
        public SoundClip ElevatorEnd;
        [Space]
        public SoundClip ElevatorOpenClean;
        public SoundClip ElevatorOpenBeep;
        public SoundClip ElevatorClose;

        public UnityEvent OnElevatorEnter;
        public UnityEvent OnElevatorExit;
        public UnityEvent OnElevatorEndMove;
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