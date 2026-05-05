using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Menu Text Colors")]
    [Summary("Tự động thay đổi màu sắc của Text dựa trên trạng thái tương tác của Button đi kèm.")]
    public class MenuTextColors : MonoBehaviour
    {
        [Tooltip("Text cần đổi màu.")]
        public TMP_Text Text;
        [Tooltip("Button dùng để kiểm tra trạng thái tương tác (interactable).")]
        public Button TextButton;
        [Tooltip("Màu sắc khi Button có thể tương tác.")]
        public Color NormalColor;
        [Tooltip("Màu sắc khi Button bị vô hiệu hóa.")]
        public Color DisabledColor;

        private void Update()
        {
            if (Text == null || TextButton == null)
                return;

            if (TextButton.interactable) Text.color = NormalColor;
            else Text.color = DisabledColor;
        }
    }
}