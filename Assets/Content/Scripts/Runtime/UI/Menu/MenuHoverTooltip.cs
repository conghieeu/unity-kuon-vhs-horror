using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Menu Hover Tooltip")]
    [Summary("Hiển thị một thông báo hướng dẫn (Tooltip) khi người chơi di chuột qua một nút bấm trong menu.")]
    public class MenuHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Nút bấm mục tiêu.")]
        public Button ButtonHover;
        [Tooltip("Text UI hiển thị nội dung hướng dẫn.")]
        public TMP_Text TooltipText;
        [Tooltip("Nội dung hướng dẫn.")]
        public GString TooltipMessage;

        private bool isHover;

        private void Awake()
        {
            TooltipMessage.SubscribeGloc(text =>
            {
                if (isHover) TooltipText.text = text;
            });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ButtonHover != null && !ButtonHover.interactable)
                return;

            TooltipText.text = TooltipMessage;
            isHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHover = false;
        }
    }
}