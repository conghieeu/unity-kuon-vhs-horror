using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    public abstract class AIStateAsset : StateAsset
    {
        /// <summary>
        /// Initialize and get FSM AI State.
        /// </summary>
        public abstract FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group);
    }
}