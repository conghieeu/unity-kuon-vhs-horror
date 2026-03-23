using System;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Inventory", menuName = "UHFPS/Game/Inventory Asset")]
    public class InventoryAsset : ScriptableObject
    {
        [Serializable]
        public struct ReferencedItem
        {
            public string guid;
            public Item item;
        }

        public List<ReferencedItem> Items = new List<ReferencedItem>();
    }
}