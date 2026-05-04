using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "ObjectReferences", menuName = "UHFPS/Object References")]
    public class ObjectReferences : ScriptableObject
    {
        public List<ObjectGuidPair> References = new List<ObjectGuidPair>();

        public ObjectGuidPair? GetObjectReference(string guid)
        {
            foreach (var elm in References)
            {
                if (elm.GUID == guid)
                {
                    return elm;
                }
            }

            return null;
        }

        public bool HasReference(string guid)
        {
            foreach (var elm in References)
            {
                if (elm.GUID == guid)
                    return true;
            }

            return false;
        }

        public bool HasReference(GameObject obj)
        {
            foreach (var elm in References)
            {
                if (elm.Object == obj)
                    return true;
            }

            return false;
        }
    
        [System.Serializable]
        public struct ObjectGuidPair
        {
            public string GUID;
            public GameObject Object;
        }
    }
}