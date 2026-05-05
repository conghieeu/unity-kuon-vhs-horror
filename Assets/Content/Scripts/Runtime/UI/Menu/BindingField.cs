using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Đại diện cho một trường hiển thị thông tin gán phím (Binding) trong menu cài đặt.")]
    public class BindingField : MonoBehaviour
    {
        [Tooltip("Tên của hành động (Action).")]
        public TMP_Text BindingName;
        [Tooltip("Tên phím đã gán cho hành động này.")]
        public TMP_Text BindingControl;
        [Tooltip("Nút bấm để bắt đầu quá trình gán lại phím (Rebind).")]
        public Button RebindControlButton;
    }
}