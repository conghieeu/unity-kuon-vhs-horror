using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using UHFPS.Tools;
using TMPro;

namespace UHFPS.Runtime
{
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        public Image ButtonImage;
        public TMP_Text ButtonText;

        public bool Interactable = true;
        public bool AutoDeselectOther = false;

        public bool UseFade = false;
        public float FadeSpeed = 3f;

        public bool Pulsating = false;
        public Color PulseColor = Color.white;
        public float PulseSpeed = 1f;
        [Range(0f, 1f)]
        public float PulseBlend = 0.5f;

        public Color ButtonNormal = Color.white;
        public Color ButtonHover = Color.white;
        public Color ButtonPressed = Color.white;
        public Color ButtonSelected = Color.white;

        public Color TextNormal = Color.white;
        public Color TextHover = Color.white;
        public Color TextPressed = Color.white;
        public Color TextSelected = Color.white;

        public UnityEvent<UIButton> OnClick;

        private bool isSelected;
        private Color textColor;

        private Color setButtonColor;
        private Color currButtonColor;
        private Color ButtonColor
        {
            get => currButtonColor;
            set
            {
                setButtonColor = value;
                currButtonColor = value;
            }
        }

        private void Awake()
        {
            ButtonColor = ButtonNormal;
            textColor = TextNormal;
        }

        private void Update()
        {
            if (Pulsating && isSelected)
            {
                float pulseBlend = GameTools.PingPong(0f, PulseBlend, PulseSpeed);
                currButtonColor = Color.Lerp(setButtonColor, PulseColor, pulseBlend);
            }

            if (UseFade)
            {
                if (ButtonImage != null) ButtonImage.color = Color.Lerp(ButtonImage.color, ButtonColor, Time.deltaTime * FadeSpeed);
                if (ButtonText != null) ButtonText.color = textColor;
            }
            else
            {
                if (ButtonImage != null) ButtonImage.color = ButtonColor;
                if (ButtonText != null) ButtonText.color = textColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Interactable || isSelected)
                return;

            ButtonColor = ButtonHover;
            textColor = TextHover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Interactable || isSelected)
                return;

            ButtonColor = ButtonNormal;
            textColor = TextNormal;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            ButtonColor = ButtonPressed;
            textColor = TextPressed;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            if (AutoDeselectOther)
            {
                foreach (var button in transform.parent.GetComponentsInChildren<UIButton>())
                {
                    if (button == this)
                        continue;

                    button.DeselectButton();
                }
            }

            ButtonColor = ButtonSelected;
            textColor = TextSelected;
            OnClick?.Invoke(this);
            isSelected = true;
        }

        public void SelectButton(bool forceColor = false)
        {
            ButtonColor = ButtonSelected;
            textColor = TextSelected;
            isSelected = true;

            if (forceColor)
            {
                if (ButtonImage != null) ButtonImage.color = ButtonColor;
                if (ButtonText != null) ButtonText.color = textColor;
            }
        }

        public void DeselectButton(bool forceColor = false)
        {
            ButtonColor = ButtonNormal;
            textColor = TextNormal;
            isSelected = false;

            if (forceColor)
            {
                if (ButtonImage != null) ButtonImage.color = ButtonColor;
                if (ButtonText != null) ButtonText.color = textColor;
            }
        }
    }
}