using System;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Generates a Unique ID that can be used to identify scripts when saving/loading script state.
    /// </summary>
    [Serializable]
    public sealed class UniqueID
    {
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