using TMPro;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Lớp cơ sở (Base Class) cho tất cả các UI script điều khiển Tuỳ chọn (Ví dụ: Slider, Toggle, Radio).")]
    public abstract class OptionBehaviour : MonoBehaviour
    {
        [Tooltip("Trạng thái cho biết người chơi có thay đổi giá trị của tuỳ chọn này hay chưa.")]
        public bool IsChanged;

        [Tooltip("Thành phần UI Text hiển thị tên/tiêu đề của tuỳ chọn.")]
        public TMP_Text Title;

        public abstract object GetOptionValue();
        public abstract void SetOptionValue(object value);
        public abstract void SetOptionData(StorableCollection data);
    }
}