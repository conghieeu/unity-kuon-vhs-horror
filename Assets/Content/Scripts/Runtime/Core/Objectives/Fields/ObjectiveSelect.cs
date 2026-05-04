using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class ObjectiveSelect
    {
        public string ObjectiveKey = "";
        public string[] SubObjectives = new string[0];
    }
}