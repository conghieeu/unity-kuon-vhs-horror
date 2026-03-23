using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using static UHFPS.Scriptable.GameLocaizationTable;
using static UHFPS.Editors.LocalizationTableWindow;

namespace UHFPS.Editors
{
    public class LanguagesTreeView : TreeView
    {
        private const string k_DeleteCommand = "Delete";
        private const string k_SoftDeleteCommand = "SoftDelete";
        private const string k_NewLanguage = "New Language";

        public Action<WindowSelection> OnLanguageSelect;

        private readonly LocalizationWindowData windowData;

        internal class LanguageTreeViewItem : TreeViewItem
        {
            public TempLanguageData language;

            public LanguageTreeViewItem(int id, int depth, TempLanguageData language) : base(id, depth, language.Entry.LanguageName)
            {
                this.language = language;
            }
        }

        public LanguagesTreeView(TreeViewState state, LocalizationWindowData data) : base(state)
        {
            windowData = data;
            rowHeight = 20f;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Languages" };
            int id = 1;

            foreach (var lang in windowData.Languages)
            {
                root.AddChild(new LanguageTreeViewItem(id++, 1, lang));
            }

            if (root.children == null)
                root.children = new List<TreeViewItem>();

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;
            var rect = args.rowRect;

            GUIContent labelIcon = EditorGUIUtility.TrTextContentWithIcon(" " + item.displayName, "BuildSettings.Web.Small");
            Rect labelRect = new(rect.x + 2f, rect.y, rect.width - 2f, rect.height);
            EditorGUI.LabelField(labelRect, labelIcon);
        }

        public override void OnGUI(Rect rect)
        {
            Rect headerRect = EditorDrawing.DrawHeaderWithBorder(ref rect, new GUIContent("LANGUAGES"), 20f, false);
            headerRect.xMin = headerRect.xMax - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
            headerRect.width = EditorGUIUtility.singleLineHeight;
            headerRect.y += EditorGUIUtility.standardVerticalSpacing;

            if (GUI.Button(headerRect, EditorUtils.Styles.PlusIcon, EditorStyles.iconButton))
            {
                OnAddNewLanguage();
                Reload();
            }

            HandleCommandEvent(Event.current);
            base.OnGUI(rect);
        }

        private void OnAddNewLanguage()
        {
            windowData.AddLanguage(k_NewLanguage, null);
        }

        protected override bool CanRename(TreeViewItem item) => true;
        protected override bool CanMultiSelect(TreeViewItem item) => true;

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename)
                return;

            var renamedItem = FindItem(args.itemID, rootItem);
            if (renamedItem == null) return;

            renamedItem.displayName = args.newName;
            if (renamedItem is LanguageTreeViewItem item)
                item.language.Entry.LanguageName = args.newName;
        }

        protected override void SingleClickedItem(int id)
        {
            var selectedItem = FindItem(id, rootItem);
            if (selectedItem != null)
            {
                if (selectedItem is LanguageTreeViewItem item)
                {
                    OnLanguageSelect?.Invoke(new LanguageSelect()
                    {
                        Language = item.language,
                        TreeViewItem = item
                    });
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count > 1)
                OnLanguageSelect?.Invoke(null);
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
            DragAndDrop.SetGenericData("Type", "Languages");
            DragAndDrop.StartDrag("Languages");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            int[] draggedIDs = (int[])DragAndDrop.GetGenericData("IDs");
            string type = (string)DragAndDrop.GetGenericData("Type");

            if (!type.Equals("Languages"))
                return DragAndDropVisualMode.Rejected;

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.BetweenItems:
                    bool acceptDrag = false;
                    foreach (var draggedId in draggedIDs)
                    {
                        var draggedItem = FindItem(draggedId, rootItem);
                        if (draggedItem != null && draggedItem is LanguageTreeViewItem lang)
                        {
                            if (args.performDrop) 
                                OnMoveLanguage(draggedId - 1, args.insertAtIndex);
                            acceptDrag = true;
                        }
                    }

                    if (args.performDrop && acceptDrag)
                    {
                        Reload();
                        SetSelection(new int[0]);
                    }

                    return acceptDrag
                        ? DragAndDropVisualMode.Move
                        : DragAndDropVisualMode.Rejected;

                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.OutsideItems:
                    break;
            }

            return DragAndDropVisualMode.Rejected;
        }

        private void OnMoveLanguage(int fromIndex, int toIndex)
        {
            int insertTo = toIndex > fromIndex ? toIndex - 1 : toIndex;
            insertTo = Mathf.Clamp(insertTo, 0, windowData.Languages.Count);

            var item = windowData.Languages[fromIndex];
            windowData.Languages.RemoveAt(fromIndex);
            windowData.Languages.Insert(insertTo, item);
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
            if (!HasFocus())
                return;

            var toDelete = GetSelection().OrderByDescending(i => i);
            if (toDelete.Count() <= 0) return;

            foreach (var index in toDelete)
            {
                windowData.Languages.RemoveAt(index - 1);
            }

            OnLanguageSelect?.Invoke(null);
            SetSelection(new int[0]);
            Reload();
        }
    }
}
