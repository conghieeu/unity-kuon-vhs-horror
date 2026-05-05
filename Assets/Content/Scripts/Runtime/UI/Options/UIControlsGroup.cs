using UnityEngine;
using UHFPS.Input;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("UI Controls Group")]
    [HelpBox("This component does not have any extra functionality other than a method to restore the default input bindings.")]
    [Summary("Nhóm các điều khiển UI liên quan đến Input, cung cấp phương thức để reset toàn bộ gán phím về mặc định.")]
    public class UIControlsGroup : MonoBehaviour
    {
        public void ResetBindings()
        {
            InputManager.ResetInputsToDefaults();
        }
    }
}