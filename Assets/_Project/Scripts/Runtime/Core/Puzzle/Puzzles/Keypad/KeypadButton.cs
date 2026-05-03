using UnityEngine;

namespace UHFPS.Runtime
{
    public class KeypadButton : MonoBehaviour, IInteractStart
    {
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