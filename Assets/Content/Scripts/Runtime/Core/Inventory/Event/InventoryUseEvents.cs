using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Inventory Use Events")]
    [Summary("Quản lý danh sách các sự kiện (Event) khi người chơi chọn Use (Sử dụng) một vật phẩm bất kỳ từ túi đồ.")]
    public class InventoryUseEvents : MonoBehaviour
    {
        [Serializable]
        public struct UseEvent
        {
            [Tooltip("ID của vật phẩm muốn đăng ký sự kiện sử dụng.")]
            public ItemGuid Item;
            [Space]
            [Tooltip("Sự kiện gọi ra khi vật phẩm được sử dụng (truyền ra thông tin Item và CustomData).")]
            public UnityEvent<ItemUseEvent> OnUse;
        }

        [Tooltip("Danh sách các sự kiện được liên kết với vật phẩm.")]
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