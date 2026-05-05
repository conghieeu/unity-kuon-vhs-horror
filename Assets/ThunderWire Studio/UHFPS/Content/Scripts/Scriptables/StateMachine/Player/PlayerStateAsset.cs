using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    public abstract class PlayerStateAsset : StateAsset
    {
        /// <summary>
        /// Initialize and get FSM Player State.
        /// </summary>
        public abstract FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group);
    }
}