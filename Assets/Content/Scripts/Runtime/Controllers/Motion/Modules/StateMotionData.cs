using System;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Lưu trữ danh sách các module chuyển động tương ứng với một trạng thái (State) cụ thể.")]
    public sealed class StateMotionData
    {
        [PlayerStatePicker(includeDefault = true)]
        public string StateID = "Default";

        [SerializeReference]
        public List<MotionModule> Motions = new();
    }
}