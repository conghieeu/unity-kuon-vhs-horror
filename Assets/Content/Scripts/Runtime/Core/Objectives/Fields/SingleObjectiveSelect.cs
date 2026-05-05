using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class SingleObjectiveSelect
    {
        [Tooltip("Khóa định danh (ID) của nhiệm vụ chính cần chọn.")]
        public string ObjectiveKey;

        [Tooltip("Khóa định danh (ID) của một mục tiêu phụ cụ thể.")]
        public string SubObjectiveKey;

        public bool IsObjValid => !string.IsNullOrEmpty(ObjectiveKey);
        public bool IsSubValid => !string.IsNullOrEmpty(SubObjectiveKey);
        public bool IsValid => IsObjValid && IsSubValid;

        public bool CompareObj(string key) => ObjectiveKey.Equals(key);
        public bool CompareSub(string key) => SubObjectiveKey.Equals(key);
    }
}