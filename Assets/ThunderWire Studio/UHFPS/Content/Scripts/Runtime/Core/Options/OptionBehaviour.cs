using TMPro;
using UnityEngine;

namespace UHFPS.Runtime
{
    public abstract class OptionBehaviour : MonoBehaviour
    {
        public bool IsChanged;
        public TMP_Text Title;

        public abstract object GetOptionValue();
        public abstract void SetOptionValue(object value);
        public abstract void SetOptionData(StorableCollection data);
    }
}