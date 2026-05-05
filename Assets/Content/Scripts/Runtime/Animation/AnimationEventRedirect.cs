using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Chuyển hướng (Redirect) các Animation Event thành Unity Event để dễ dàng gọi các hàm từ script khác.")]
    public class AnimationEventRedirect : MonoBehaviour
    {
        [System.Serializable]
        public struct AnimationEvent
        {
            [Tooltip("Tên của sự kiện Animation cần nhận (phải khớp với tên được truyền trong Animation Clip).")]
            public string Name;
            [Tooltip("Sự kiện Unity (Unity Event) sẽ được kích hoạt khi nhận được Animation Event.")]
            public UnityEvent OnCallEvent;
        }

        [Tooltip("Danh sách các sự kiện chuyển hướng từ Animation Event sang Unity Event.")]
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