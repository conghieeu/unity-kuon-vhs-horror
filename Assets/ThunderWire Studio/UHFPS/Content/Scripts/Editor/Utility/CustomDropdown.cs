using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

namespace UHFPS.Editors
{
    public struct CustomDropdownItem
    {
        public string Path;
        public object Item;
        public string Icon;

        public CustomDropdownItem(string path, object item)
        {
            Path = path;
            Item = item;
            Icon = "";
        }

        public CustomDropdownItem(string path, object item, string icon)
        {
            Path = path;
            Item = item;
            Icon = icon;
        }
    }

    public class CustomDropdown : AdvancedDropdown
    {
        private readonly IEnumerable<CustomDropdownItem> items;
        private readonly string dropdownName;

        public Action<CustomDropdownItem> OnItemSelected;

        private class DropdownItem : AdvancedDropdownItem
        {
            public CustomDropdownItem item;

            public DropdownItem(string displayName, CustomDropdownItem item) : base(displayName)
            {
                this.item = item;
            }
        }

        public CustomDropdown(AdvancedDropdownState state, string dropdownName, IEnumerable<CustomDropdownItem> items) : base(state)
        {
            this.items = items;
            this.dropdownName = dropdownName;
            minimumSize = new Vector2(minimumSize.x, 270f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(dropdownName);
            var groupMap = new Dictionary<string, AdvancedDropdownItem>();

            foreach (var item in items)
            {
                // split the name into groups
                string path = item.Path;
                string[] groups = path.Split('/');

                // create or find the groups
                AdvancedDropdownItem parent = root;
                for (int i = 0; i < groups.Length - 1; i++)
                {
                    string groupPath = string.Join("/", groups.Take(i + 1));
                    if (!groupMap.ContainsKey(groupPath))
                    {
                        var newGroup = new AdvancedDropdownItem(groups[i]);
                        parent.AddChild(newGroup);
                        groupMap[groupPath] = newGroup;
                    }
                    parent = groupMap[groupPath];
                }

                // create the item and add it to the last group
                DropdownItem dropItem = new(groups.Last(), item);
                if(!string.IsNullOrEmpty(item.Icon))
                {
                    Texture icon = EditorGUIUtility.TrIconContent(item.Icon).image;
                    dropItem.icon = icon as Texture2D;
                }

                parent.AddChild(dropItem);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            DropdownItem element = (DropdownItem)item;
            OnItemSelected?.Invoke(element.item);
        }
    }
}