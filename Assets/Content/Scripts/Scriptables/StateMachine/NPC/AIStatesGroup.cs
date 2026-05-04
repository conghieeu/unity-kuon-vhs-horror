using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [Serializable]
    public struct AIStateData
    {
        public AIStateAsset StateAsset;
        public bool IsEnabled;
    }

    public abstract class AIStatesGroup : ScriptableObject
    {
        public List<AIStateData> AIStates = new List<AIStateData>();

        public List<NPCStateMachine.State> GetStates(NPCStateMachine machine)
        {
            return AIStates.Select(x => new NPCStateMachine.State()
            {
                StateData = x,
                FSMState = x.StateAsset.InitState(machine, this)
            }).ToList();
        }
    }
}