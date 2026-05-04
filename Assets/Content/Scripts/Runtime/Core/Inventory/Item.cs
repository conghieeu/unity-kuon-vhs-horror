using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    public enum ImageOrientation { Normal, Flipped };
    public enum FlipDirection { Left, Right };
    public enum UsableType { PlayerItem, HealthItem, CustomEvent }

    [Serializable]
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
        public ItemUsableSettings UsableSettings;

        [Serializable]
        public struct ItemProperties
        {
            public ushort maxStack;
        }
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

            [Tooltip("Use crafting like operations. If you combine two items, their quantity is reduced by the required crafting amount and the resulting item quantity will be set from the resulting item amount.")]
            public bool isCrafting;
            [Tooltip("After combining, do not remove the active item from inventory.")]
            public bool keepAfterCombine;
            [Tooltip("After combining, remove the second item from the inventory.")]
            public bool removeSecondItem;
            [Tooltip("After combining, call the combine event if the second inventory item is a player item. The combine event will be called only on the second item.")]
            public bool eventAfterCombine;
            [Tooltip("After combining, select the player item instead of adding the result item to the inventory.")]
            public bool selectAfterCombine;
            [Tooltip("After combining, the resulting item will have custom data.")]
            public bool haveCustomData;
        }
        public ItemCombineSettings[] CombineSettings;

        [Serializable]
        public struct Localization
        {
            public GString titleKey;
            public GString descriptionKey;
        }
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