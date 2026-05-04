using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Objectives", menuName = "UHFPS/Game/Objectives Asset")]
    public class ObjectivesAsset : ScriptableObject
    {
        public List<Objective> Objectives = new List<Objective>();

        public bool ContainsObjective(string key)
        {
            if(string.IsNullOrEmpty(key)) return false;
            return Objectives.Any(x => x.ObjectiveKey == key);
        }

        public bool ContainsSubObjective(string objKey, string subKey)
        {
            if (string.IsNullOrEmpty(objKey) || string.IsNullOrEmpty(subKey)) 
                return false;

            foreach (var obj in Objectives)
            {
                if (obj.ObjectiveKey == objKey)
                {
                    return obj.SubObjectives.Any(x => x.SubObjectiveKey == subKey);
                }
            }

            return false;
        }
    }
}