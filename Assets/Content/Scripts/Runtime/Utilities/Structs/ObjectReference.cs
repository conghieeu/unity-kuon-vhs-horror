using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class ObjectReference
    {
        public string GUID;
        public GameObject Object;
    }
}