using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct Objective
    {
        [Tooltip("Khóa định danh (ID) của nhiệm vụ chính.")]
        public string ObjectiveKey;

        [Tooltip("Tiêu đề của nhiệm vụ chính (Hỗ trợ Localization).")]
        public GString ObjectiveTitle;

        [Tooltip("Danh sách các mục tiêu phụ (Sub-objectives) của nhiệm vụ này.")]
        public SubObjective[] SubObjectives;
    }

    [Serializable]
    public struct SubObjective
    {
        [Tooltip("Khóa định danh (ID) của mục tiêu phụ.")]
        public string SubObjectiveKey;

        [Tooltip("Số lượng cần hoàn thành (Ví dụ: Nhặt 3 chìa khóa -> Số lượng là 3).")]
        public ushort CompleteCount;

        [Tooltip("Nội dung text của mục tiêu phụ (Hỗ trợ Localization).")]
        public GString ObjectiveText;
    }
}