using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public abstract class OptionModule
    {
        public abstract string ContextName { get; }
        public OptionsManager Options { get; set; }
        public OptionBehaviour Behaviour { get; set; }

        public string OptionName => ContextName.Split("/").Last();
        public bool IsChanged => Behaviour.IsChanged;
        public object Value => Behaviour.GetOptionValue();

        public string GUID;
        public string Name;
        public GameObject Prefab;
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
    public class OptionSeparator : OptionModule
    {
        public override string ContextName => "Separator";

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "text", Title } });
        }
    }
}