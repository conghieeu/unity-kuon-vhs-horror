using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct Tag
    {
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