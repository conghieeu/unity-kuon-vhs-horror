using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThunderWire.Attributes;
using TMPro;

namespace UHFPS.Runtime
{
    [InspectorHeader("Menu Hover Tooltip")]
    public class MenuHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button ButtonHover;
        public TMP_Text TooltipText;
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