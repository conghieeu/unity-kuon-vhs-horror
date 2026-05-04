using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct Objective
    {
        public string ObjectiveKey;
        public GString ObjectiveTitle;
        public SubObjective[] SubObjectives;
    }

    [Serializable]
    public struct SubObjective
    {
        public string SubObjectiveKey;
        public ushort CompleteCount;
        public GString ObjectiveText;
    }
}