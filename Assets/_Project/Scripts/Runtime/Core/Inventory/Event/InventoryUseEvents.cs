using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Inventory Use Events")]
    public class InventoryUseEvents : MonoBehaviour
    {
        [Serializable]
        public struct UseEvent
        {
            public ItemGuid Item;
            [Space]
            public UnityEvent<ItemUseEvent> OnUse;
        }

        public List<UseEvent> UseEvents = new();

        private void Start()
        {
            Inventory inventory = Inventory.Instance;

            foreach (var evt in UseEvents)
            {
                inventory.RegisterUseEvent(evt.Item, act => evt.OnUse?.Invoke(act));
            }
        }
    }
}