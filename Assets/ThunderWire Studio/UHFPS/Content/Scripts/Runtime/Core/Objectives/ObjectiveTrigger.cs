using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class ObjectiveTrigger : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum TriggerType { Trigger, Interact, Event }
        public enum ObjectiveType { New, Complete, NewAndComplete }

        public TriggerType triggerType = TriggerType.Trigger;
        public ObjectiveType objectiveType = ObjectiveType.New;

        public ObjectiveSelect objectiveToAdd;
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