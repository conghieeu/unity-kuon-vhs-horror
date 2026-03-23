using UnityEngine;
using UHFPS.Scriptable;

namespace UHFPS.Runtime
{
    public static class SerializationUtillity
    {
        private static SerializationAsset serializationAsset;
        public static SerializationAsset SerializationAsset
        {
            get
            {
                if (serializationAsset == null)
                    serializationAsset = Resources.Load<SerializationAsset>("Serialization Asset");

                return serializationAsset;
            }
        }
    }
}