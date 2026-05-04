using UnityEngine.UI;
using UnityEngine;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Interact Button")]
    public class InteractButton : MonoBehaviour
    {
        public GameObject Separator;
        public TMP_Text InteractInfo;
        public Image ButtonImage;
        public Vector2 ButtonSize;

        private RectTransform buttonRect;
        private LayoutElement buttonLayout;

        public void SetButton(string name, Sprite button, Vector2 scale)
        {
            if(buttonRect == null)
                buttonRect = ButtonImage.rectTransform;

            if (buttonLayout == null)
                buttonLayout = ButtonImage.GetComponent<LayoutElement>();

            if (Separator != null)
                Separator.SetActive(true);

            gameObject.SetActive(true);
            InteractInfo.text = name;
            ButtonImage.sprite = button;

            buttonRect.sizeDelta = ButtonSize * scale;
            buttonLayout.preferredWidth = ButtonSize.x * scale.x;
            buttonLayout.preferredHeight = ButtonSize.y * scale.y;
        }

        public void HideButton()
        {
            gameObject.SetActive(false);
            if (Separator != null) 
                Separator.SetActive(false);
        }
    }
}