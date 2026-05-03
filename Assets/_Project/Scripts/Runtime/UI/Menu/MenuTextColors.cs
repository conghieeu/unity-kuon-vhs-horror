using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Menu Text Colors")]
    public class MenuTextColors : MonoBehaviour
    {
        public TMP_Text Text;
        public Button TextButton;
        public Color NormalColor;
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