using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Options Escape Events")]
    [Summary("Xử lý các sự kiện khi người chơi nhấn ESC trong menu tùy chọn.")]
    public class OptionsEscapeEvents : MonoBehaviour
    {
        [Tooltip("Hủy bỏ mọi thay đổi chưa lưu khi nhấn ESC.")]
        public bool DiscardChanges;
        [Space]
        [Tooltip("Các sự kiện bổ sung sẽ được gọi khi nhấn ESC.")]
        public UnityEvent EscapeEvents;

        private OptionsManager optionsManager;

        private void Awake()
        {
            optionsManager = OptionsManager.Instance;
            GameManager.SubscribePauseEvent(esc =>
            {
                if (!esc) OnEscape();
            });
        }

        public void OnEscape()
        {
            EscapeEvents?.Invoke();
            if (DiscardChanges)
                optionsManager.DiscardChanges();
        }
    }
}