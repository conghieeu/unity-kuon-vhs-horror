using UnityEngine;

namespace UHFPS.Scriptable
{
    public abstract class StateAsset : ScriptableObject 
    {
        /// <summary>
        /// Get a state key to help recognize which state is currently active.
        /// <br>Override it to define your own state key.</br>
        /// </summary>
        public virtual string StateKey => ToString();

        /// <summary>
        /// Get FSM State display name.
        /// </summary>
        public virtual string Name => GetType().Name;
    }
}