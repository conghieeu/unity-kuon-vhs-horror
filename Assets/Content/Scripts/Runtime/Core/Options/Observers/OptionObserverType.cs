using System;
using System.Linq;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Lớp cơ sở trừu tượng cho một loại hành động lắng nghe Option (Ví dụ: Chỉnh FOV, Chỉnh Volume).")]
    public abstract class OptionObserverType
    {
        [Tooltip("Tên định danh (Name) của Custom Option cần lắng nghe.")]
        public string ObserveOptionName;

        public abstract string Name { get; }
        public virtual void OnStart() { }
        public abstract void OptionUpdate(object value);

        public override string ToString() => Name.Split('/').Last();
    }
}