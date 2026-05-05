using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Thành phần gán vào các nút bấm vật lý của hệ thống bảng mã PIN (Keypad).")]
    public class KeypadButton : MonoBehaviour, IInteractStart
    {
        [Tooltip("Định nghĩa loại nút bấm (Số từ 0-9, Clear, Enter).")]
        public KeypadPuzzle.Button Button = KeypadPuzzle.Button.Number0;
        
        private KeypadPuzzle keypadPuzzle;

        private void Start()
        {
            keypadPuzzle = transform.GetComponentInParent<KeypadPuzzle>();
        }

        public void InteractStart()
        {
            keypadPuzzle.OnPressButton(Button);
        }
    }
}