using System;
using UHFPS.Scriptable;
using UnityEngine;

namespace UHFPS.Runtime
{
    public sealed class Transition
    {
        public Type NextStateType { get; private set; }
        public string NextStateKey { get; private set; }

        public Func<bool> Condition { get; private set; }
        public bool Value => Condition.Invoke();

        public static Transition To<TState>(Func<bool> condition) where TState : StateAsset
        {
            StateAsset scriptableObject = ScriptableObject.CreateInstance<TState>();
            string stateName = scriptableObject.StateKey;
            UnityEngine.Object.Destroy(scriptableObject);

            return new Transition()
            {
                NextStateType = typeof(TState),
                NextStateKey = stateName,
                Condition = condition
            };
        }

        public static Transition To(string stateKey, Func<bool> condition)
        {
            return new Transition()
            {
                NextStateType = null,
                NextStateKey = stateKey,
                Condition = condition
            };
        }

        public static Transition Back(Func<bool> condition)
        {
            return new Transition()
            {
                NextStateType = null,
                NextStateKey = PlayerStateMachine.PREVIOUS_STATE,
                Condition = condition
            };
        }
    }
}