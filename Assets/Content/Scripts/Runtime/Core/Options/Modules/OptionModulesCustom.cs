using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Tuỳ chọn loại True/False (Bật/Tắt) tùy chỉnh. Thường dùng cho các cấu hình như Bật/Tắt VHS Effect, Subtitles...")]
    public class OptionCustomBoolean : OptionModule
    {
        public override string ContextName => "Custom/Boolean";

        [Tooltip("Giá trị mặc định khi game khởi tạo hoặc khi Reset tuỳ chọn.")]
        public bool DefaultValue;

        [Tooltip("Chuỗi text hiển thị khi tuỳ chọn đang ở trạng thái Tắt (Ví dụ: OFF, KHÔNG).")]
        public GString OffName = new("*OFF", "");

        [Tooltip("Chuỗi text hiển thị khi tuỳ chọn đang ở trạng thái Bật (Ví dụ: ON, CÓ).")]
        public GString OnName = new("*ON", "");

        public override void OnApplyOption()
        {
            int value = (int)Value;
            bool converted = value != 0;

            if (Behaviour.IsChanged && Options.OptionSubjects.TryGetValue(Name, out var subject))
                subject.OnNext(converted);

            Options.SerializableData[Name] = new(converted);
        }

        public override void OnLoadOption(bool fromFile)
        {
            bool optionValue = DefaultValue;

            if(fromFile && CheckOption(JTokenType.Boolean, out bool value))
                optionValue = value;

            if (Options.OptionSubjects.TryGetValue(Name, out var subject))
                subject.OnNext(optionValue);

            Behaviour.SetOptionValue(optionValue);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection()
            {
                { "options", new GString[] { OffName, OnName } },
                { "defaultValue", DefaultValue ? 1 : 0 }
            });
        }
    }

    [Serializable]
    [Summary("Tuỳ chọn loại Float tùy chỉnh (Dạng thanh kéo Slider). Thường dùng cho các giá trị như Âm lượng, Độ sáng...")]
    public class OptionCustomFloat : OptionModule
    {
        public override string ContextName => "Custom/Float";

        [Tooltip("Giá trị mặc định khi game khởi tạo hoặc khi Reset tuỳ chọn.")]
        public float DefaultValue;

        [Tooltip("Giới hạn giá trị nhỏ nhất và lớn nhất của thanh kéo.")]
        public MinMax SliderLimits = new(0, 1);

        [Tooltip("Bật chế độ Snapping (hít) để giá trị tăng/giảm theo từng bậc thay vì liên tục.")]
        public bool UseSnapping = true;

        [Tooltip("Giá trị của mỗi bậc khi hít (Ví dụ: 0.05).")]
        public float SnapValue = 0.05f;

        public override void OnApplyOption()
        {
            float value = (float)Value;
            if (Behaviour.IsChanged && Options.OptionSubjects.TryGetValue(Name, out var subject))
                subject.OnNext(value);

            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            float optionValue = DefaultValue;

            if (fromFile && CheckOption(JTokenType.Float, out float value))
                optionValue = value;

            if (Options.OptionSubjects.TryGetValue(Name, out var subject))
                subject.OnNext(optionValue);

            Behaviour.SetOptionValue(optionValue);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection()
            {
                { "settings", new object[]
                    {
                        OptionsSlider.SliderTypeEnum.FloatSlider,
                        SliderLimits,
                        UseSnapping,
                        SnapValue
                    }
                },
                { "defaultValue", DefaultValue }
            });
        }
    }

    [Serializable]
    [Summary("Tuỳ chọn loại Số nguyên (Integer) tùy chỉnh (Dạng thanh kéo). Thường dùng cho FOV, Giới hạn FPS...")]
    public class OptionCustomInteger : OptionModule
    {
        public override string ContextName => "Custom/Integer";

        [Tooltip("Giá trị mặc định khi game khởi tạo hoặc khi Reset tuỳ chọn.")]
        public int DefaultValue;

        [Tooltip("Giới hạn nhỏ nhất và lớn nhất của thanh kéo (Ví dụ: 60 - 120 cho FOV).")]
        public MinMaxInt SliderLimits = new(0, 1);

        public override void OnApplyOption()
        {
            int value = Convert.ToInt32(Value);
            if (Behaviour.IsChanged && Options.OptionSubjects.TryGetValue(Name, out var subject))
                subject.OnNext(value);

            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            int optionValue = DefaultValue;

            if (fromFile && CheckOption(JTokenType.Integer, out int value))
                optionValue = value;

            if (Options.OptionSubjects.TryGetValue(Name, out var subject))
                subject.OnNext(optionValue);

            Behaviour.SetOptionValue((float)optionValue);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection()
            {
                { "settings", new object[]
                    {
                        OptionsSlider.SliderTypeEnum.IntegerSlider,
                        new MinMax(SliderLimits.min, SliderLimits.max),
                        false,
                        0f
                    }
                },
                { "defaultValue", (float)DefaultValue }
            });
        }
    }
}