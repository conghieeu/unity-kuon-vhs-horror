using System;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public class OptionCustomBoolean : OptionModule
    {
        public override string ContextName => "Custom/Boolean";

        public bool DefaultValue;
        public GString OffName = new("*OFF", "");
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
    public class OptionCustomFloat : OptionModule
    {
        public override string ContextName => "Custom/Float";

        public float DefaultValue;
        public MinMax SliderLimits = new(0, 1);
        public bool UseSnapping = true;
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
    public class OptionCustomInteger : OptionModule
    {
        public override string ContextName => "Custom/Integer";

        public int DefaultValue;
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