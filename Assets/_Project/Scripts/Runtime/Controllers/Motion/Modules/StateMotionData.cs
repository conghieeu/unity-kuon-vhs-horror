using System;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class StateMotionData
    {
        [PlayerStatePicker(includeDefault = true)]
        public string StateID = "Default";

        [SerializeReference]
        public List<MotionModule> Motions = new();
    }
}