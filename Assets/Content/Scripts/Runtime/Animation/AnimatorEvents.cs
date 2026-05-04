using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class AnimatorEvents : MonoBehaviour
    {
        public Animator Animator;

        private readonly List<string> toggledParameters = new();

        public void ToggleBool(string name)
        {
            if (!toggledParameters.Contains(name))
            {
                Animator.SetBool(name, true);
                toggledParameters.Add(name);
            }
            else
            {
                Animator.SetBool(name, false);
                toggledParameters.Remove(name);
            }
        }

        public void SetBoolTrue(string name)
        {
            Animator.SetBool(name, true);
        }

        public void SetBoolFalse(string name)
        {
            Animator.SetBool(name, false);
        }
    }
}