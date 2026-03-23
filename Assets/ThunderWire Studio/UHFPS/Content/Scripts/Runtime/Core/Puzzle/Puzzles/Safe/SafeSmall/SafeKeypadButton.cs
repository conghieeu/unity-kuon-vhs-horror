using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Safe Keypad Button")]
    public class SafeKeypadButton : MonoBehaviour, IInteractStart
    {
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