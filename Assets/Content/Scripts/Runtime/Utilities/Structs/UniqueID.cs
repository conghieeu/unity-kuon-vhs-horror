using System;
using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Tạo một mã định danh duy nhất (Unique ID) để nhận diện các script khi lưu/tải trạng thái.
    /// </summary>
    [Serializable]
    [Summary("Mã định danh duy nhất cho Save/Load.")]
    public sealed class UniqueID
    {
        [Tooltip("Mã định danh duy nhất.")]
        public string Id;

        public UniqueID()
        {
            GenerateIfEmpty();
        }

        /// <summary>
        /// Generate an ID only if it's missing.
        /// </summary>
        public void GenerateIfEmpty()
        {
            if (!string.IsNullOrEmpty(Id)) 
                return;

            Generate();
        }

        /// <summary>
        /// Assign a new random ID and overwrite the previous.
        /// </summary>
        public void Generate()
        {
            Id = GameTools.GetGuid();
        }

        public static implicit operator string(UniqueID uniqueID)
        {
            return uniqueID.Id;
        }

        public override string ToString() => Id;
    }
}