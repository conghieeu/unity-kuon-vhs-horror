using UnityEngine;
using UnityEngine.Events;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Component tự động cập nhật Text UI mỗi khi có thay đổi ngôn ngữ (Localization).")]
    public class GLocText : MonoBehaviour
    {
        [Tooltip("Khóa Localization cần lấy văn bản dịch (Ví dụ: ui.menu.play).")]
        public GString GlocKey;

        [Tooltip("Nếu đúng, cho phép theo dõi và định dạng nhiều giá trị cùng lúc trong một chuỗi.")]
        public bool ObserveMany;

        [Tooltip("Sự kiện trả về chuỗi văn bản đã được dịch để hiển thị lên Text.")]
        public UnityEvent<string> OnUpdateText;

        private void Start()
        {
            if (!GameLocalization.HasReference)
                return;

            if (!ObserveMany) GlocKey.SubscribeGloc(text => OnUpdateText?.Invoke(text));
            else GlocKey.SubscribeGlocMany(text => OnUpdateText?.Invoke(text));
        }
    }
}