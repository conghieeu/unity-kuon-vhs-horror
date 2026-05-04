using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct NameValue<TValue> where TValue : struct
    {
        public GString Name;
        public TValue Value;

        public NameValue(GString name, TValue value)
        {
            Name = new(name, "");
            Value = value;
        }
    }
}