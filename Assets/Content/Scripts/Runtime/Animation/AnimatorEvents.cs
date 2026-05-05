using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Cung cấp các hàm tiện ích để điều khiển Animator thông qua Unity Events (Ví dụ: ToggleBool, SetBoolTrue).")]
    public class AnimatorEvents : MonoBehaviour
    {
        [Tooltip("Animator mà script này sẽ điều khiển.")]
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