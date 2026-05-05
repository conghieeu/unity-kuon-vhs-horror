using System;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    public enum ImageOrientation { Normal, Flipped };
    public enum FlipDirection { Left, Right };
    public enum UsableType { PlayerItem, HealthItem, CustomEvent }

    [Serializable]
    [Summary("Định nghĩa toàn bộ thuộc tính và cài đặt của một Vật phẩm (Item) trong hệ thống Inventory.")]
    public sealed class Item
    {
        public string GUID;
        public string SectionGUID;

        public string Title;
        public string Description;
        public ushort Width;
        public ushort Height;
        public ImageOrientation Orientation;
        public FlipDirection FlipDirection;
        public Sprite Icon;

        public ObjectReference ItemObject;

        [Serializable]
        public struct ItemSettings 
        {
            public bool isUsable;
            public bool isStackable;
            public bool isExaminable;
            public bool isCombinable;
            public bool isDroppable;
            public bool isDiscardable;
            public bool canBindShortcut;
            public bool alwaysShowQuantity;
        }
        [Tooltip("Các cài đặt chung của vật phẩm (Có thể dùng, xếp chồng, kết hợp, v.v.).")]
        public ItemSettings Settings;

        [Serializable]
        public struct ItemUsableSettings
        {
            public UsableType usableType;
            public int playerItemIndex;
            public uint healthPoints;

            public bool removeOnUse;
            public ItemCustomData customData;
        }
        [Tooltip("Cài đặt khi vật phẩm được Use (Dùng làm máu, chạy sự kiện, v.v.).")]
        public ItemUsableSettings UsableSettings;

        [Serializable]
        public struct ItemProperties
        {
            public ushort maxStack;
        }
        [Tooltip("Thuộc tính riêng (Ví dụ số lượng tối đa xếp chồng).")]
        public ItemProperties Properties;

        [Serializable]
        public struct ItemCombineSettings
        {
            public ushort requiredCurrentAmount;
            public ushort requiredSecondAmount;
            public ushort resultItemAmount;

            public string combineWithID;
            public string resultCombineID;
            public int playerItemIndex;

            public bool inheritCustomData;
            public bool inheritFromSecond;
            public string inheritKey;
            public ItemCustomData customData;

            [Tooltip("Sử dụng cơ chế chế tạo (Crafting). Nếu kết hợp 2 vật phẩm, số lượng sẽ bị trừ đi theo lượng yêu cầu, và tạo ra vật phẩm mới theo số lượng kết quả.")]
            public bool isCrafting;
            [Tooltip("Sau khi kết hợp, KHÔNG xóa vật phẩm đang chọn khỏi túi đồ.")]
            public bool keepAfterCombine;
            [Tooltip("Sau khi kết hợp, XÓA vật phẩm thứ 2 (vật phẩm bị mang ra kết hợp) khỏi túi đồ.")]
            public bool removeSecondItem;
            [Tooltip("Gọi sự kiện Combine nếu vật phẩm thứ 2 là Player Item (vật phẩm cầm trên tay).")]
            public bool eventAfterCombine;
            [Tooltip("Sau khi kết hợp, lấy luôn vật phẩm mới ra tay thay vì cất vào túi đồ.")]
            public bool selectAfterCombine;
            [Tooltip("Vật phẩm tạo ra sau khi kết hợp sẽ mang dữ liệu tùy chỉnh (Custom Data).")]
            public bool haveCustomData;
        }
        [Tooltip("Cài đặt kết hợp vật phẩm (Crafting/Combine).")]
        public ItemCombineSettings[] CombineSettings;

        [Serializable]
        public struct Localization
        {
            public GString titleKey;
            public GString descriptionKey;
        }
        [Tooltip("Cài đặt Đa ngôn ngữ (Localization) cho tên và mô tả vật phẩm.")]
        public Localization LocalizationSettings;

        /// <summary>
        /// Creates a new instance of a class with the same values as an existing instance.
        /// </summary>
        public Item DeepCopy()
        {
            return new Item()
            {
                Title = Title,
                Description = Description,
                Width = Width,
                Height = Height,
                Orientation = Orientation,
                FlipDirection = FlipDirection,
                Icon = Icon,
                ItemObject = ItemObject,
                Settings = Settings,
                UsableSettings = UsableSettings,
                Properties = Properties,
                CombineSettings = CombineSettings,
                LocalizationSettings = LocalizationSettings
            };
        }
    }
}