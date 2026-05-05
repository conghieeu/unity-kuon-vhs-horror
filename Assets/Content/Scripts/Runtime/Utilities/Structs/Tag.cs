using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Wrapper cho Tag của Unity, hỗ trợ các phép toán so sánh và chuyển đổi.
    /// </summary>
    [Serializable]
    public struct Tag
    {
        [Tooltip("Tên thẻ (Tag).")]
        public string tag;

        public static implicit operator string(Tag tag)
        {
            return tag.tag;
        }

        public static implicit operator Tag(string tag)
        {
            Tag result = default;
            result.tag = tag;
            return result;
        }

        public bool CompareTag(GameObject obj)
        {
            return obj.CompareTag(this);
        }
    }
}