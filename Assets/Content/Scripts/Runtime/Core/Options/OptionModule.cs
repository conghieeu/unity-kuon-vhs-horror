using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Lớp cơ sở trừu tượng đại diện cho một Tuỳ chọn (Cấu hình) trong Settings. Chứa logic xử lý lưu/tải và áp dụng giá trị.")]
    public abstract class OptionModule
    {
        public abstract string ContextName { get; }
        public OptionsManager Options { get; set; }
        public OptionBehaviour Behaviour { get; set; }

        public string OptionName => ContextName.Split("/").Last();
        public bool IsChanged => Behaviour.IsChanged;
        public object Value => Behaviour.GetOptionValue();

        [Tooltip("Chuỗi ID định danh duy nhất của tuỳ chọn.")]
        public string GUID;

        [Tooltip("Tên định danh của tuỳ chọn (Dùng làm Key để lưu trong file Save).")]
        public string Name;

        [Tooltip("Prefab giao diện UI tương ứng với loại tuỳ chọn này (Ví dụ: Slider_Prefab).")]
        public GameObject Prefab;

        [Tooltip("Tiêu đề hiển thị lên giao diện UI của tuỳ chọn này.")]
        public GString Title;

        public virtual void OnApplyOption() { }
        public virtual void OnLoadOption(bool fromFile) { }
        public virtual void OnBuildOption(OptionBehaviour behaviour) { }
        public virtual void OnBuildOptionRuntime() { }

        protected bool CheckOption<T>(JTokenType type, out T value) where T : struct
        {
            if (Options.SerializableData.TryGetValue(Name, out JValue jValue) && jValue.Type == type)
            {
                value = jValue.ToObject<T>();
                return true;
            }

            value = default;
            return false;
        }

        protected bool CheckOption<T>(string name, JTokenType type, out T value) where T : struct
        {
            if (Options.SerializableData.TryGetValue(name, out JValue jValue) && jValue.Type == type)
            {
                value = jValue.ToObject<T>();
                return true;
            }

            value = default;
            return false;
        }

        protected OptionBehaviour GetPrevious()
        {
            if (!OptionsManager.HasReference)
                return null;

            return OptionsManager.Instance.GetPreviousOption();
        }
    }

    [Serializable]
    [Summary("Tuỳ chọn loại Phân cách (Separator), dùng để nhóm các tuỳ chọn lại bằng một thanh tiêu đề ngang.")]
    public class OptionSeparator : OptionModule
    {
        public override string ContextName => "Separator";

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "text", Title } });
        }
    }
}