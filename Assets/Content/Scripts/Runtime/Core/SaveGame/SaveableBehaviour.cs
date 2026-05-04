using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Derive from this class if you want to define a saveable object that can be instantiated at runtime.
    /// </summary>
    public abstract class SaveableBehaviour : MonoBehaviour, IRuntimeSaveable
    {
        /// <summary>
        /// A unique ID that is used to determine which object has been instantiated.
        /// </summary>
        /// <remarks>The object must be added to the ObjectReferences asset.</remarks>
        [field: SerializeField]
        public UniqueID UniqueID { get; set; }

        public abstract StorableCollection OnSave();

        public abstract void OnLoad(JToken data);
    }
}