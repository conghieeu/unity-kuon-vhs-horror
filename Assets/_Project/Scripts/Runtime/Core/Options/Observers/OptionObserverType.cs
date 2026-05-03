using System;
using System.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public abstract class OptionObserverType
    {
        public string ObserveOptionName;

        public abstract string Name { get; }
        public virtual void OnStart() { }
        public abstract void OptionUpdate(object value);

        public override string ToString() => Name.Split('/').Last();
    }
}