using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Inventory", menuName = "UHFPS/Game/Inventory Database")]
    public class InventoryDatabase : ScriptableObject
    {
        [Serializable]
        public sealed class Section
        {
            public string Name;
            public string GUID;
        }

        [Serializable]
        public sealed class ItemsSection
        {
            public Section Section;
            public List<Item> Items;
        }

        public List<ItemsSection> Sections = new();

        public List<Item> Items =>
            Sections.SelectMany(x => x.Items).ToList();

        public ItemsSection GetSection(string guid)
        {
            foreach (var section in Sections)
            {
                if (section.Section.GUID == guid)
                {
                    return section;
                }
            }

            return null;
        }

        public Item GetItem(string guid)
        {
            foreach (var section in Sections)
            {
                foreach (var item in section.Items)
                {
                    if (item.GUID == guid)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public bool TryGetItemByGUID(string guid, out Item item)
        {
            item = GetItem(guid);
            return item != null;
        }

        public bool TryGetItemWithSection(string itemGUID, out Section section, out Item item)
        {
            foreach (var _section in Sections)
            {
                foreach (var _item in _section.Items)
                {
                    if (_item.GUID == itemGUID)
                    {
                        section = _section.Section;
                        item = _item;
                        return true;
                    }
                }
            }

            section = null;
            item = null;
            return false;
        }
    }
}