using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class ObjectiveEvent : MonoBehaviour
    {
        public SingleObjectiveSelect Objective;
        
        public UnityEvent OnObjectiveAdded;
        public UnityEvent OnObjectiveCompleted;

        public UnityEvent OnSubObjectiveAdded;
        public UnityEvent OnSubObjectiveCompleted;
        public UnityEvent<int> OnSubObjectiveCountChanged;
    }
}