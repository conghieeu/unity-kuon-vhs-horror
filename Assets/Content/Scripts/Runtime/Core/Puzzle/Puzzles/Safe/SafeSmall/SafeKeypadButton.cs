using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Safe Keypad Button")]
    [Summary("Thành phần gán vào nút bấm trên bảng điều khiển điện tử của két sắt nhỏ.")]
    public class SafeKeypadButton : MonoBehaviour, IInteractStart
    {
        [Tooltip("Định nghĩa chức năng của nút bấm này (0-9, Clear, Enter).")]
        public SafeKeypadPuzzle.Button Button = SafeKeypadPuzzle.Button.Number0;
        private SafeKeypadPuzzle safePuzzle;

        private void Start()
        {
            safePuzzle = transform.GetComponentInParent<SafeKeypadPuzzle>();
        }

        public void InteractStart()
        {
            safePuzzle.OnPressButton(Button);
        }
    }
}