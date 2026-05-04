using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class AnimationEventRedirect : MonoBehaviour
    {
        [System.Serializable]
        public struct AnimationEvent
        {
            public string Name;
            public UnityEvent OnCallEvent;
        }

        public List<AnimationEvent> AnimationEvents = new();

        public void CallEvent(string name)
        {
            foreach (var evt in AnimationEvents)
            {
                if(evt.Name == name)
                {
                    evt.OnCallEvent?.Invoke();
                    break;
                }
            }
        }
    }
}