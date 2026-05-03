using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [Serializable]
    public struct PlayerStateData
    {
        public PlayerStateAsset stateAsset;
        public bool isEnabled;
    }

    public abstract class PlayerStatesGroup : ScriptableObject
    {
        public List<PlayerStateData> PlayerStates = new List<PlayerStateData>();

        public List<PlayerStateMachine.State> GetStates(PlayerStateMachine machine)
        {
            return PlayerStates.Select(x => new PlayerStateMachine.State()
            {
                stateData = x,
                fsmState = x.stateAsset.InitState(machine, this)
            }).ToList();
        }
    }
}