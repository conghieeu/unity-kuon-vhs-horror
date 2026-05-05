using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Tham chiếu đến một GameObject kèm theo mã GUID.
    /// </summary>
    [Serializable]
    [Summary("Tham chiếu đối tượng theo GUID.")]
    public sealed class ObjectReference
    {
        [Tooltip("Mã định danh duy nhất (GUID) của đối tượng.")]
        public string GUID;
        [Tooltip("GameObject tham chiếu.")]
        public GameObject Object;
    }
}