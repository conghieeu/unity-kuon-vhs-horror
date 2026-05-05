using UnityEngine;
using UnityEngine.Events;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý các sự kiện (UnityEvent) liên quan đến một Nhiệm vụ (Objective) cụ thể, như khi nhận nhiệm vụ hoặc hoàn thành.")]
    public class ObjectiveEvent : MonoBehaviour
    {
        [Tooltip("Nhiệm vụ cần lắng nghe sự kiện.")]
        public SingleObjectiveSelect Objective;
        
        [Tooltip("Sự kiện gọi ra khi nhận được Nhiệm vụ này.")]
        public UnityEvent OnObjectiveAdded;

        [Tooltip("Sự kiện gọi ra khi Nhiệm vụ này hoàn thành.")]
        public UnityEvent OnObjectiveCompleted;

        [Tooltip("Sự kiện gọi ra khi nhận được một Nhiệm vụ phụ (Sub-Objective).")]
        public UnityEvent OnSubObjectiveAdded;

        [Tooltip("Sự kiện gọi ra khi một Nhiệm vụ phụ hoàn thành.")]
        public UnityEvent OnSubObjectiveCompleted;

        [Tooltip("Sự kiện gọi ra khi số lượng Nhiệm vụ phụ thay đổi (Ví dụ: thu thập 1/3 vật phẩm). Truyền ra số lượng hiện tại.")]
        public UnityEvent<int> OnSubObjectiveCountChanged;
    }
}