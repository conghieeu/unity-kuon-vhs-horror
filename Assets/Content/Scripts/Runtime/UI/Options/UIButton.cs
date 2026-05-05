using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using UHFPS.Tools;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Một component Button tùy chỉnh hỗ trợ hiệu ứng chuyển màu, làm mờ (Fade) và rung động (Pulsating) nâng cao.")]
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        [Tooltip("Image của nút.")]
        public Image ButtonImage;
        [Tooltip("Text của nút.")]
        public TMP_Text ButtonText;

        [Tooltip("Nút này có thể tương tác được không.")]
        public bool Interactable = true;
        [Tooltip("Tự động bỏ chọn các UIButton khác cùng cấp khi nút này được nhấn.")]
        public bool AutoDeselectOther = false;

        [Tooltip("Sử dụng hiệu ứng chuyển màu mượt mà (Fade) thay vì đổi màu tức thì.")]
        public bool UseFade = false;
        [Tooltip("Tốc độ chuyển màu.")]
        public float FadeSpeed = 3f;

        [Tooltip("Tạo hiệu ứng rung động màu sắc khi nút được chọn.")]
        public bool Pulsating = false;
        [Tooltip("Màu sắc mục tiêu của hiệu ứng rung động.")]
        public Color PulseColor = Color.white;
        [Tooltip("Tốc độ rung động.")]
        public float PulseSpeed = 1f;
        [Range(0f, 1f)]
        [Tooltip("Cường độ pha trộn của hiệu ứng rung động.")]
        public float PulseBlend = 0.5f;

        [Tooltip("Màu sắc nút ở trạng thái bình thường.")]
        public Color ButtonNormal = Color.white;
        [Tooltip("Màu sắc nút khi di chuột qua.")]
        public Color ButtonHover = Color.white;
        [Tooltip("Màu sắc nút khi nhấn giữ.")]
        public Color ButtonPressed = Color.white;
        [Tooltip("Màu sắc nút khi được chọn.")]
        public Color ButtonSelected = Color.white;

        [Tooltip("Màu sắc text ở trạng thái bình thường.")]
        public Color TextNormal = Color.white;
        [Tooltip("Màu sắc text khi di chuột qua.")]
        public Color TextHover = Color.white;
        [Tooltip("Màu sắc text khi nhấn giữ.")]
        public Color TextPressed = Color.white;
        [Tooltip("Màu sắc text khi được chọn.")]
        public Color TextSelected = Color.white;

        [Tooltip("Sự kiện gọi ra khi nút được nhấn.")]
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