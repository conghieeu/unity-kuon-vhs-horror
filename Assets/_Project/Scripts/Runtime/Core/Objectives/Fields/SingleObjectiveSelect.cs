using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class SingleObjectiveSelect
    {
        public string ObjectiveKey;
        public string SubObjectiveKey;

        public bool IsObjValid => !string.IsNullOrEmpty(ObjectiveKey);
        public bool IsSubValid => !string.IsNullOrEmpty(SubObjectiveKey);
        public bool IsValid => IsObjValid && IsSubValid;

        public bool CompareObj(string key) => ObjectiveKey.Equals(key);
        public bool CompareSub(string key) => SubObjectiveKey.Equals(key);
    }
}