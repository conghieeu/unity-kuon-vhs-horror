using Newtonsoft.Json.Linq;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Trigger dùng để thêm mới hoặc hoàn thành nhiệm vụ khi người chơi va chạm (Trigger), tương tác (Interact) hoặc gọi qua Event.")]
    public class ObjectiveTrigger : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum TriggerType { Trigger, Interact, Event }
        public enum ObjectiveType { New, Complete, NewAndComplete }

        [Tooltip("Cách thức kích hoạt Trigger (Va chạm, Tương tác, hoặc Gọi qua Sự kiện).")]
        public TriggerType triggerType = TriggerType.Trigger;

        [Tooltip("Hành động xử lý nhiệm vụ (Thêm mới, Hoàn thành, hoặc Cả hai).")]
        public ObjectiveType objectiveType = ObjectiveType.New;

        [Tooltip("Dữ liệu nhiệm vụ sẽ được THÊM MỚI khi kích hoạt.")]
        public ObjectiveSelect objectiveToAdd;

        [Tooltip("Dữ liệu nhiệm vụ sẽ được HOÀN THÀNH khi kích hoạt.")]
        public ObjectiveSelect objectiveToComplete;

        private bool isTriggered;

        private ObjectiveManager objectiveManager;
        private ObjectiveManager ObjectiveManager
        {
            get
            {
                if(objectiveManager == null)
                    objectiveManager = ObjectiveManager.Instance;

                return objectiveManager;
            }
        }

        public void InteractStart()
        {
            if (triggerType != TriggerType.Interact || triggerType == TriggerType.Event || isTriggered)
                return;

            TriggerObjective();
            isTriggered = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerType != TriggerType.Trigger || triggerType == TriggerType.Event || isTriggered)
                return;

            if (other.CompareTag("Player"))
            {
                TriggerObjective();
                isTriggered = true;
            }
        }

        public void TriggerObjective()
        {
            if (objectiveType == ObjectiveType.New)
            {
                ObjectiveManager.AddObjective(objectiveToAdd.ObjectiveKey, objectiveToAdd.SubObjectives);
            }
            else if (objectiveType == ObjectiveType.Complete)
            {
                ObjectiveManager.CompleteObjective(objectiveToComplete.ObjectiveKey, objectiveToComplete.SubObjectives);
            }
            else if(objectiveType == ObjectiveType.NewAndComplete)
            {
                ObjectiveManager.AddObjective(objectiveToAdd.ObjectiveKey, objectiveToAdd.SubObjectives);
                ObjectiveManager.CompleteObjective(objectiveToComplete.ObjectiveKey, objectiveToComplete.SubObjectives);
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isTriggered), isTriggered }
            };
        }

        public void OnLoad(JToken data)
        {
            isTriggered = (bool)data[nameof(isTriggered)];
        }
    }
}