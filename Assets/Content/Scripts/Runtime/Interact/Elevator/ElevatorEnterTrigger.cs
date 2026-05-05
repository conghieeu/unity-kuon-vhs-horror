using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Elevator Enter Trigger")]
    [HelpBox("This script is used to determine whether the player is in the elevator or not. It sends the elevator enter event to the parent elevator script and sets the player parent to the elevator.")]
    [Summary("Quản lý việc phát hiện người chơi bước vào/ra thang máy, đồng thời gán người chơi làm con (child) của thang máy để di chuyển cùng.")]
    public class ElevatorEnterTrigger : MonoBehaviour
    {
        [Space]
        [Tooltip("Transform chứa người chơi khi họ bước vào (Thường là sàn của thang máy).")]
        public Transform ElevatorParent;
        private ElevatorSystem elevatorSystem;

        private void Awake()
        {
            elevatorSystem = gameObject.GetComponentInParent<ElevatorSystem>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                elevatorSystem.OnElevatorTriggerEnter(true);
                PlayerManager.Instance.ParentToObject(ElevatorParent);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                elevatorSystem.OnElevatorTriggerEnter(false);
                PlayerManager.Instance.UnparentFromObject();
            }
        }
    }
}