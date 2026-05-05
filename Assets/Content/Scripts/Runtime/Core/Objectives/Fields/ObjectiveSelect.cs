using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class ObjectiveSelect
    {
        [Tooltip("Khóa định danh (ID) của nhiệm vụ chính cần chọn.")]
        public string ObjectiveKey = "";

        [Tooltip("Danh sách các mục tiêu phụ (Sub-objectives) được chọn.")]
        public string[] SubObjectives = new string[0];
    }
}