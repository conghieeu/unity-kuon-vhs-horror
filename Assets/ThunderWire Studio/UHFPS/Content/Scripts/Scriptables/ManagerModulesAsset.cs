using System.Collections.Generic;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "ManagerModules", menuName = "UHFPS/Manager Modules")]
    public class ManagerModulesAsset : ScriptableObject
    {
        [SerializeReference]
        public List<ManagerModule> ManagerModules = new();
    }
}