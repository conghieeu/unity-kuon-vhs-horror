using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Option Observer")]
    [HelpBox("Observes the change in the custom option value, which will be assigned to a specific reflection type using reflection.")]
    [Summary("Component lắng nghe sự thay đổi của một Custom Option và gán giá trị đó vào một biến/hàm bất kỳ thông qua Reflection.")]
    public class OptionObserver : MonoBehaviour
    {
        [Space]
        [Tooltip("Tên định danh (Name) của Custom Option cần lắng nghe (Ví dụ: 'vhs_effect').")]
        public string OptionName;
        [Space]
        [Tooltip("Biến hoặc Thuộc tính (Property) sẽ nhận giá trị mới khi Option thay đổi.")]
        public GenericReflectionField OptionAction;

        private void Start()
        {
            OptionsManager.ObserveOption(OptionName, (obj) => OptionAction.Value = obj);
        }
    }
}