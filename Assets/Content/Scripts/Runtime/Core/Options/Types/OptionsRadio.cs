using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UHFPS.Tools;
using ThunderWire.Attributes;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UHFPS.Runtime
{
    [InspectorHeader("Options Radio")]
    [Summary("Điều khiển UI cho một tùy chọn dạng Radio (Nhiều lựa chọn chữ viết, chọn bằng nút mũi tên trái/phải).")]
    public class OptionsRadio : OptionBehaviour
    {
        [Tooltip("Text dùng để hiển thị giá trị đang được chọn (VD: Thấp, Trung bình, Cao).")]
        public TMP_Text RadioText;

        [Tooltip("Script xử lý hình ảnh chấm/vạch sáng biểu thị vị trí đang chọn.")]
        public OptionIndicator Indicator;

        [Header("Radio Settings")]
        [Tooltip("Vị trí (Index) lựa chọn mặc định ban đầu.")]
        public int RadioIndex = 0;

        [Tooltip("Bật nếu danh sách lựa chọn sẽ được nạp bằng Code thay vì gõ tay trong Inspector.")]
        public bool OptionsCustom;

        [Tooltip("Danh sách các giá trị văn bản của lựa chọn (Hỗ trợ Localization).")]
        public GString[] Options;

        [Header("Events")]
        [Tooltip("Sự kiện gọi ra khi giá trị thay đổi (Trả về Index mới).")]
        public UnityEvent<int> OnChange;

        private void Start()
        {
            if (OptionsCustom || Options.Length <= 0)
                return;

            bool listenToChange = false;
            for (int i = 0; i < Options.Length; i++)
            {
                Options[i].SubscribeGloc(txt =>
                {
                    if (!listenToChange)
                        return;

                    int index = i;
                    if (RadioIndex == index)
                        RadioText.text = txt;
                });
            }

            SetOption(RadioIndex);
            listenToChange = true;
        }

        public override void SetOptionValue(object value)
        {
            int radio = Convert.ToInt32(value);
            SetOption(radio);
            IsChanged = false;
        }

        public override object GetOptionValue()
        {
            return RadioIndex;
        }

        public override void SetOptionData(StorableCollection data)
        {
            if(data.TryGetValue("options", out GString[] options))
                Options = options.Select(x => new GString(x)).ToArray();

            if (data.TryGetValue("defaultValue", out int value))
                RadioIndex = value;

#if UNITY_EDITOR
            if (Indicator != null)
            {
                GameObject indicator = Indicator.Indicators[0].gameObject;

                // clear existing indicators except the first one
                Indicator.Indicators.Clear();
                Indicator.Indicators.Add(indicator.GetComponent<Image>());

                for (int i = 1; i < Options.Length; i++)
                {
                    GameObject newIndicator = GameObjectUtility.DuplicateGameObject(indicator);
                    Image image = newIndicator.GetComponent<Image>();
                    Indicator.Indicators.Add(image);
                }

                // ensure the indicators are in the correct order
                Indicator.Indicators = Indicator.Indicators.OrderBy(ind => ind.transform.GetSiblingIndex()).ToList();
            }
#endif
        }

        public void ChangeOption(int change)
        {
            int nextOption = GameTools.Wrap(RadioIndex + change, 0, Options.Length);
            SetOption(nextOption);
        }

        public void SetOption(int index)
        {
            if(Options.Length > index)
                RadioText.text = Options[index];

            RadioIndex = index;
            OnChange?.Invoke(index);
            IsChanged = true;
        }
    }
}