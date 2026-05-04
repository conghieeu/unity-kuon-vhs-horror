using UnityEngine;

namespace ThunderWire.Attributes
{
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public bool IsLabel = false;

        public ReadOnlyAttribute(bool label = false)
        {
            IsLabel = label;
        }
    }
}