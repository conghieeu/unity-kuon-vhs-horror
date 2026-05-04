using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using static UHFPS.Editors.InventoryBuilder;

namespace UHFPS.Editors
{
    public class ItemsTreeView : TreeView
    {
        private const string k_DeleteCommand = "Delete";
        private const string k_SoftDeleteCommand = "SoftDelete";

        public Action OnAddNewSection;
        public Action<SelectionArgs?> OnItemSelect;
        public Action<string> OnAddNewItem;
        public Action<string> OnDeleteSection;
        public Action<string, string> OnDeleteItem;

        public Action<string, string, int> OnMoveItem;
        public Action<string, string, string> OnMoveItemToSection;
        public Action<string, string, string, int> OnMoveItemToSectionAt;
        public Action<string, int> OnMoveSection;
        public Action OnRebuild;

        private readonly BuilderData builderData;

        private bool InitiateContextMenuOnNextRepaint = false;
        private int ContextSelectedID = -1;

        internal class InventoryTreeViewItem : TreeViewItem
        {
            public BuilderItem Data;
            public SerializedProperty Title;

            public InventoryTreeViewItem(int id, int depth, BuilderItem item) : base(id, depth, item.Title)
            {
                Data = item;
                Title = item.Properties["Title"];
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

        public ItemsTreeView(TreeViewState viewState, BuilderData builderData) : base(viewState)
        {
            this.builderData = builderData;
            rowHeight = 20f;
            Reload();
        }

        private void PopUpContextMenu()
        {
            var selectedItem = FindItem(ContextSelectedID, rootItem);
            var menu = new GenericMenu();

            if (selectedItem is SectionTreeViewItem section)
            {
                menu.AddItem(new GUIContent("Add Item"), false, () =>
                {
                    OnAddNewItem?.Invoke(section.Data.GUID);
                    OnRebuild?.Invoke();
                    ContextSelectedID = -1;
                });

                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    OnDeleteSection?.Invoke(section.Data.GUID);
                    OnRebuild?.Invoke();
                });
            }
            else if (selectedItem is InventoryTreeViewItem invItem)
            {
                SectionTreeViewItem parentSection = (SectionTreeViewItem)invItem.parent;
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    OnDeleteItem?.Invoke(parentSection.Data.GUID, invItem.Data.GUID);
                    OnRebuild?.Invoke();
                });
            }

            menu.ShowAsContext();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Inventory" };
            int id = 1;

            foreach (var section in builderData.Sections)
            {
                string sectionName = section.SectionName.stringValue;
                var sectionItem = new SectionTreeViewItem(id++, 0, section);

                root.AddChild(sectionItem);

                // Add items within each section as children of the section.
                foreach (var item in section.Items)
                {
                    string itemName = item.Properties["Title"].stringValue;
                    sectionItem.AddChild(new InventoryTreeViewItem(id++, 1, item));
                }
            }

            if (root.children == null)
                root.children = new List<TreeViewItem>();

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;
            var rect = args.rowRect;

            string iconName = item is SectionTreeViewItem ? "Folder Icon" : "Prefab On Icon";
            GUIContent labelIcon = EditorGUIUtility.TrTextContentWithIcon(" " + item.displayName, iconName);

            Rect labelRect = new(rect.x + GetContentIndent(item), rect.y, rect.width - GetContentIndent(item), rect.height);
            EditorGUI.LabelField(labelRect, labelIcon);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            GUIContent headerTitle = EditorGUIUtility.TrTextContentWithIcon(" INVENTORY ITEMS", "Prefab On Icon");
            Rect headerRect = EditorDrawing.DrawHeaderWithBorder(ref rect, headerTitle, 20f, false);

            headerRect.xMin = headerRect.xMax - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
            headerRect.width = EditorGUIUtility.singleLineHeight;
            headerRect.y += EditorGUIUtility.standardVerticalSpacing;

            GUIContent plusIcon = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add Item");
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

        protected override bool CanRename(TreeViewItem item) => true;

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename)
                return;

            var renamedItem = FindItem(args.itemID, rootItem);
            if (renamedItem == null) return;

            renamedItem.displayName = args.newName;
            if (renamedItem is InventoryTreeViewItem item)
            {
                item.Title.stringValue = args.newName;
            }
            else if (renamedItem is SectionTreeViewItem section)
            {
                section.Section.stringValue = args.newName;
            }

            builderData.SerializedObject.ApplyModifiedProperties();
            builderData.SerializedObject.Update();
        }

        protected override bool CanMultiSelect(TreeViewItem item) => true;

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            var firstItem = FindItem(args.draggedItemIDs[0], rootItem);
            return args.draggedItemIDs.All(id => FindItem(id, rootItem).parent == firstItem.parent);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 1)
            {
                var selectedItem = FindItem(selectedIds[0], rootItem);
                if (selectedItem != null)
                {
                    if (selectedItem is InventoryTreeViewItem item)
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

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData("IDs", args.draggedItemIDs.ToArray());
            DragAndDrop.SetGenericData("Type", "InventoryItems");
            DragAndDrop.StartDrag("Items");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            int[] draggedIDs = (int[])DragAndDrop.GetGenericData("IDs");
            string type = (string)DragAndDrop.GetGenericData("Type");

            if (!type.Equals("InventoryItems"))
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
                            if (draggedItem != null && draggedItem is InventoryTreeViewItem item)
                            {
                                if (args.performDrop)
                                {
                                    if (draggedItem.parent == section1)
                                    {
                                        OnMoveItem?.Invoke(section1.Data.GUID, item.Data.GUID, args.insertAtIndex);
                                    }
                                    else
                                    {
                                        var parentSection = (SectionTreeViewItem)draggedItem.parent;
                                        OnMoveItemToSectionAt?.Invoke(parentSection.Data.GUID, item.Data.GUID, section1.Data.GUID, args.insertAtIndex);
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
                                    OnMoveSection?.Invoke(section.Data.GUID, args.insertAtIndex);
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
                            if (draggedItem != null && draggedItem is InventoryTreeViewItem item)
                            {
                                if (args.performDrop && draggedItem.parent != section2)
                                {
                                    var parentSection = (SectionTreeViewItem)draggedItem.parent;
                                    OnMoveItemToSection?.Invoke(parentSection.Data.GUID, item.Data.GUID, section2.Data.GUID);
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

                if (selectedItem is InventoryTreeViewItem item)
                {
                    var parentSection = (SectionTreeViewItem)selectedItem.parent;
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