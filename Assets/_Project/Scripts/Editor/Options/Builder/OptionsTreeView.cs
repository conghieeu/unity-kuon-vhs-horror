using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using static UHFPS.Editors.OptionsBuilder;

namespace UHFPS.Editors
{
    public class OptionsTreeView : TreeView
    {
        private const string k_DeleteCommand = "Delete";
        private const string k_SoftDeleteCommand = "SoftDelete";

        public Action<SelectionArgs?> OnItemSelect;
        public Action<string, OptionModule> OnAddNewOption;
        public Action OnAddNewSection;
        public Action OnRebuild;

        public Action<string> OnDeleteSection;
        public Action<string, string> OnDeleteItem;

        private readonly BuilderData builderData;
        private List<CustomDropdownItem> options;

        private bool InitiateContextMenuOnNextRepaint = false;
        private int ContextSelectedID = -1;

        internal class OptionTreeViewItem : TreeViewItem
        {
            public BuilderItem Data;
            public SerializedProperty Name;
            public bool IsSeparator;

            public OptionTreeViewItem(int id, int depth, bool separator, BuilderItem item) : base(id, depth, item.Name)
            {
                Data = item;
                IsSeparator = separator;
                Name = item.Properties["Name"];
            }
        }

        internal class SectionTreeViewItem : TreeViewItem
        {
            public BuilderSection Data;
            public SerializedProperty Section;

            public SectionTreeViewItem(int id, int depth, BuilderSection section) : base(id, depth, section.Name)
            {
                Data = section;
                Section = section.SectionName;
            }
        }

        public OptionsTreeView(TreeViewState viewState, BuilderData builderData) : base(viewState)
        {
            this.builderData = builderData;
            rowHeight = 20f;

            InitializeOptions();
            Reload();
        }

        private void InitializeOptions()
        {
            options = new();
            foreach (var option in TypeCache.GetTypesDerivedFrom<OptionModule>().Where(x => !x.IsAbstract))
            {
                OptionModule instance = (OptionModule)Activator.CreateInstance(option);
                options.Add(new()
                {
                    Path = instance.ContextName,
                    Item = instance
                });
            }

            options = options
                .OrderByDescending(x => x.Path.Contains("/"))
                .ThenBy(x => x.Path.Contains("/") ? x.Path.Split('/')[0] : x.Path)
                .ToList();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Options" };
            int id = 1;

            foreach (var section in builderData.Sections)
            {
                string sectionName = section.SectionName.stringValue;
                var sectionItem = new SectionTreeViewItem(id++, 0, section);

                root.AddChild(sectionItem);

                // add items within each section as children of the section.
                foreach (var item in section.Items)
                {
                    string itemName = item.Properties["Name"].stringValue;
                    bool separator = item.IsSeparator;
                    sectionItem.AddChild(new OptionTreeViewItem(id++, 1, separator, item));
                }
            }

            if (root.children == null)
                root.children = new List<TreeViewItem>();

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            GUIContent headerTitle = EditorGUIUtility.TrTextContentWithIcon(" OPTIONS PROFILE", "Settings");
            Rect headerRect = EditorDrawing.DrawHeaderWithBorder(ref rect, headerTitle, 20f, false);

            headerRect.xMin = headerRect.xMax - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
            headerRect.width = EditorGUIUtility.singleLineHeight;
            headerRect.y += EditorGUIUtility.standardVerticalSpacing;

            GUIContent plusIcon = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add Section");
            if (GUI.Button(headerRect, plusIcon, EditorStyles.iconButton))
            {
                OnAddNewSection?.Invoke();
                OnRebuild?.Invoke();
            }

            if (InitiateContextMenuOnNextRepaint)
            {
                InitiateContextMenuOnNextRepaint = false;
                PopUpContextMenu();
            }

            HandleCommandEvent(Event.current);
            base.OnGUI(rect);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;
            var rect = args.rowRect;

            Rect contextIconRect = rect;
            contextIconRect.xMin = rect.xMax - EditorGUIUtility.singleLineHeight;
            contextIconRect.x -= 2f;

            GUIContent labelText = new(item.displayName);

            if (item is SectionTreeViewItem section)
            {
                labelText = EditorGUIUtility.TrTextContentWithIcon(" " + item.displayName, "Folder Icon");

                GUIContent toolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus More");
                contextIconRect.y += 2f;

                if (GUI.Button(contextIconRect, toolbarPlus, EditorStyles.iconButton))
                {
                    ShowAddOptionDropdown(rect, section);
                }
            }
            else if (item is OptionTreeViewItem option && !option.IsSeparator)
            {
                labelText = EditorGUIUtility.TrTextContentWithIcon(" " + item.displayName, "Profiler.UIDetails");
                if (option.Data.Prefab == null)
                {
                    GUIContent warningIcon = EditorGUIUtility.TrIconContent("Warning", "Prefab reference is missing!");
                    EditorGUI.LabelField(contextIconRect, warningIcon);
                }
            }
            else
            {
                if (!args.selected)
                {
                    ColorUtility.TryParseHtmlString("#645e37", out Color color);
                    color.a = 0.5f;
                    EditorGUI.DrawRect(rect, color);
                }

                labelText.text = $"[Separator] {item.displayName}";
            }

            Rect labelRect = new(rect.x + GetContentIndent(item), rect.y, rect.width - GetContentIndent(item), rect.height);
            EditorGUI.LabelField(labelRect, labelText);
        }

        private void ShowAddOptionDropdown(Rect rowRect, SectionTreeViewItem section)
        {
            Rect dropdownRect = rowRect;
            dropdownRect.width = 250f;
            dropdownRect.height = 0f;
            dropdownRect.y += 21f;
            dropdownRect.x += rowRect.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

            CustomDropdown customDropdown = new(new AdvancedDropdownState(), "Options", options);
            customDropdown.OnItemSelected = (e) =>
            {
                // create a new instance of the option to avoid reference issues
                OptionModule instance = (OptionModule)Activator.CreateInstance(e.Item.GetType());
                OnAddNewOption?.Invoke(section.Data.GUID, instance);
                ContextSelectedID = -1;
            };

            customDropdown.Show(dropdownRect);
        }

        private void PopUpContextMenu()
        {
            var selectedItem = FindItem(ContextSelectedID, rootItem);
            var menu = new GenericMenu();

            if (selectedItem is SectionTreeViewItem section)
            {
                menu.AddItem(new GUIContent("Delete Section"), false, () =>
                {
                    OnDeleteSection?.Invoke(section.Data.GUID);
                    OnRebuild?.Invoke();
                });
            }
            else if (selectedItem is OptionTreeViewItem optionItem)
            {
                if (optionItem.parent is SectionTreeViewItem parentSection)
                {
                    menu.AddItem(new GUIContent("Delete Option"), false, () =>
                    {
                        OnDeleteItem?.Invoke(parentSection.Data.GUID, optionItem.Data.GUID);
                        OnRebuild?.Invoke();
                    });
                }
            }

            menu.ShowAsContext();
        }

        protected override bool CanMultiSelect(TreeViewItem item) => true;

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 1)
            {
                var selectedItem = FindItem(selectedIds[0], rootItem);
                if (selectedItem != null)
                {
                    if (selectedItem is OptionTreeViewItem item)
                    {
                        OnItemSelect?.Invoke(new()
                        {
                            Selection = item.Data,
                            TreeViewItem = selectedItem
                        });
                    }
                    else if (selectedItem is SectionTreeViewItem section)
                    {
                        OnItemSelect?.Invoke(new()
                        {
                            Selection = section.Data,
                            TreeViewItem = selectedItem
                        });
                    }
                }
                else
                {
                    OnItemSelect?.Invoke(null);
                }
            }
            else
            {
                OnItemSelect?.Invoke(null);
            }
        }

        protected override void ContextClickedItem(int id)
        {
            InitiateContextMenuOnNextRepaint = true;
            ContextSelectedID = id;
            Repaint();
        }

        protected override bool CanRename(TreeViewItem item) => true;

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename)
                return;

            var renamedItem = FindItem(args.itemID, rootItem);
            if (renamedItem == null) return;

            renamedItem.displayName = args.newName;
            if (renamedItem is OptionTreeViewItem option)
            {
                option.Name.stringValue = args.newName;
            }
            else if (renamedItem is SectionTreeViewItem section)
            {
                section.Section.stringValue = args.newName;
            }

            builderData.SerializedObject.ApplyModifiedProperties();
            builderData.SerializedObject.Update();
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            var firstItem = FindItem(args.draggedItemIDs[0], rootItem);
            return args.draggedItemIDs.All(id => FindItem(id, rootItem).parent == firstItem.parent);
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData("IDs", args.draggedItemIDs.ToArray());
            DragAndDrop.SetGenericData("Type", "OptionsItems");
            DragAndDrop.StartDrag("Options");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            int[] draggedIDs = (int[])DragAndDrop.GetGenericData("IDs");
            string type = (string)DragAndDrop.GetGenericData("Type");

            if (!type.Equals("OptionsItems"))
                return DragAndDropVisualMode.Rejected;

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.BetweenItems:
                    if (args.parentItem is SectionTreeViewItem section1)
                    {
                        bool acceptDrag = false;
                        foreach (var draggedId in draggedIDs)
                        {
                            var draggedItem = FindItem(draggedId, rootItem);
                            if (draggedItem != null && draggedItem is OptionTreeViewItem item)
                            {
                                if (args.performDrop)
                                {
                                    if (draggedItem.parent == section1)
                                    {
                                        OnMoveItem(section1.Data.GUID, item.Data.GUID, args.insertAtIndex);
                                    }
                                    else
                                    {
                                        var parentSection = (SectionTreeViewItem)draggedItem.parent;
                                        OnMoveItemToSectionAt(parentSection.Data.GUID, item.Data.GUID, section1.Data.GUID, args.insertAtIndex);
                                    }
                                }
                                acceptDrag = true;
                            }
                        }

                        if (args.performDrop && acceptDrag)
                        {
                            OnRebuild?.Invoke();
                            SetSelection(new int[0]);
                        }

                        return acceptDrag
                            ? DragAndDropVisualMode.Move
                            : DragAndDropVisualMode.Rejected;
                    }
                    else
                    {
                        bool acceptDrag = false;
                        foreach (var draggedId in draggedIDs)
                        {
                            var draggedItem = FindItem(draggedId, rootItem);
                            if (draggedItem != null && draggedItem is SectionTreeViewItem section)
                            {
                                if (args.performDrop)
                                {
                                    OnMoveSection(section.Data.GUID, args.insertAtIndex);
                                }
                                acceptDrag = true;
                            }
                        }

                        if (args.performDrop && acceptDrag)
                        {
                            OnRebuild?.Invoke();
                            SetSelection(new int[0]);
                        }

                        return acceptDrag
                            ? DragAndDropVisualMode.Move
                            : DragAndDropVisualMode.Rejected;
                    }

                case DragAndDropPosition.UponItem:
                    if (args.parentItem is SectionTreeViewItem section2)
                    {
                        bool acceptDrag = false;
                        foreach (var draggedId in draggedIDs)
                        {
                            var draggedItem = FindItem(draggedId, rootItem);
                            if (draggedItem != null && draggedItem is OptionTreeViewItem item)
                            {
                                if (args.performDrop && draggedItem.parent != section2)
                                {
                                    var parentSection = (SectionTreeViewItem)draggedItem.parent;
                                    OnMoveItemToSection(parentSection.Data.GUID, item.Data.GUID, section2.Data.GUID);
                                }
                                acceptDrag = true;
                            }
                        }

                        if (args.performDrop && acceptDrag)
                        {
                            OnRebuild?.Invoke();
                            SetSelection(new int[0]);
                        }

                        return acceptDrag
                            ? DragAndDropVisualMode.Move
                            : DragAndDropVisualMode.Rejected;
                    }
                    break;

                case DragAndDropPosition.OutsideItems:
                    break;
            }

            return DragAndDropVisualMode.Rejected;
        }

        private void OnMoveItem(string sectionGuid, string itemGuid, int newIndex)
        {
            // find the section
            var sectionIndex = builderData.Target.Sections.FindIndex(s => s.Section.GUID == sectionGuid);
            if (sectionIndex < 0) return;

            // find the item within the section
            var section = builderData.Target.Sections[sectionIndex];
            var itemIndex = section.Items.FindIndex(i => i.GUID == itemGuid);
            if (itemIndex < 0) return;

            // move item within the list
            var item = section.Items[itemIndex];

            int insertTo = newIndex > itemIndex ? newIndex - 1 : newIndex;
            insertTo = Mathf.Clamp(insertTo, 0, section.Items.Count);

            section.Items.RemoveAt(itemIndex);
            section.Items.Insert(insertTo, item);
        }

        private void OnMoveItemToSection(string fromSectionGuid, string itemGuid, string toSectionGuid)
        {
            // find the source section and item
            var fromSectionIndex = builderData.Target.Sections.FindIndex(s => s.Section.GUID == fromSectionGuid);
            if (fromSectionIndex < 0) return;

            var fromSection = builderData.Target.Sections[fromSectionIndex];
            var itemIndex = fromSection.Items.FindIndex(i => i.GUID == itemGuid);
            if (itemIndex < 0) return;

            // remove item from source section
            var item = fromSection.Items[itemIndex];
            fromSection.Items.RemoveAt(itemIndex);

            // find the destination section
            var toSectionIndex = builderData.Target.Sections.FindIndex(s => s.Section.GUID == toSectionGuid);
            if (toSectionIndex < 0) return;

            // add item to destination section
            var toSection = builderData.Target.Sections[toSectionIndex];
            toSection.Items.Add(item);
        }

        private void OnMoveItemToSectionAt(string fromSectionGuid, string itemGuid, string toSectionGuid, int newIndex)
        {
            // find the source section and item
            var fromSectionIndex = builderData.Target.Sections.FindIndex(s => s.Section.GUID == fromSectionGuid);
            if (fromSectionIndex < 0) return;

            var fromSection = builderData.Target.Sections[fromSectionIndex];
            var itemIndex = fromSection.Items.FindIndex(i => i.GUID == itemGuid);
            if (itemIndex < 0) return;

            // remove item from source section
            var item = fromSection.Items[itemIndex];
            fromSection.Items.RemoveAt(itemIndex);

            // find the destination section
            var toSectionIndex = builderData.Target.Sections.FindIndex(s => s.Section.GUID == toSectionGuid);
            if (toSectionIndex < 0) return;

            // add item to destination section
            var toSection = builderData.Target.Sections[toSectionIndex];
            toSection.Items.Insert(newIndex, item);
        }

        private void OnMoveSection(string sectionGuid, int newIndex)
        {
            // find the section
            var sectionIndex = builderData.Target.Sections.FindIndex(s => s.Section.GUID == sectionGuid);
            if (sectionIndex < 0) return;

            // move section within the list
            var section = builderData.Target.Sections[sectionIndex];

            int insertTo = newIndex > sectionIndex ? newIndex - 1 : newIndex;
            insertTo = Mathf.Clamp(insertTo, 0, section.Items.Count);

            builderData.Target.Sections.RemoveAt(sectionIndex);
            builderData.Target.Sections.Insert(insertTo, section);
        }

        private void HandleCommandEvent(Event uiEvent)
        {
            if (uiEvent.type == EventType.ValidateCommand)
            {
                switch (uiEvent.commandName)
                {
                    case k_DeleteCommand:
                    case k_SoftDeleteCommand:
                        if (HasSelection())
                            uiEvent.Use();
                        break;
                }
            }
            else if (uiEvent.type == EventType.ExecuteCommand)
            {
                switch (uiEvent.commandName)
                {
                    case k_DeleteCommand:
                    case k_SoftDeleteCommand:
                        DeleteSelected();
                        break;
                }
            }
        }

        private void DeleteSelected()
        {
            var toDelete = GetSelection().OrderByDescending(i => i);
            if (toDelete.Count() <= 0) return;

            foreach (var index in toDelete)
            {
                var selectedItem = FindItem(index, rootItem);
                if (selectedItem == null) continue;

                if (selectedItem is OptionTreeViewItem item)
                {
                    if (selectedItem.parent is SectionTreeViewItem parentSection)
                        OnDeleteItem?.Invoke(parentSection.Data.GUID, item.Data.GUID);
                }
                else if (selectedItem is SectionTreeViewItem section)
                {
                    OnDeleteSection?.Invoke(section.Data.GUID);
                }
            }

            SetSelection(new int[0]);
            OnRebuild?.Invoke();
        }
    }
}