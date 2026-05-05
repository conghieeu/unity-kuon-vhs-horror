using UnityEngine.UI;
using UnityEngine;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Interact Button")]
    [Summary("Đại diện cho một nút bấm hiển thị thông tin tương tác (Phím + Tên hành động).")]
    public class InteractButton : MonoBehaviour
    {
        [Tooltip("Dấu ngăn cách giữa các nút.")]
        public GameObject Separator;
        [Tooltip("Text hiển thị tên hành động tương tác.")]
        public TMP_Text InteractInfo;
        [Tooltip("Image hiển thị icon của phím bấm.")]
        public Image ButtonImage;
        [Tooltip("Kích thước cơ sở của nút bấm.")]
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