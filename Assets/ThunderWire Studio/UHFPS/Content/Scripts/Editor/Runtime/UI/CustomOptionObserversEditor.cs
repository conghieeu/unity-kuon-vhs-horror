using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(CustomOptionObservers)), CanEditMultipleObjects]
    public class CustomOptionObserversEditor : InspectorEditor<CustomOptionObservers>
    {
        public struct ObserverPair
        {
            public Type observerType;
            public string observerName;
        }

        private List<ObserverPair> observers;

        public override void OnEnable()
        {
            base.OnEnable();
            observers = new();

            foreach (var type in TypeCache.GetTypesDerivedFrom<OptionObserverType>().Where(x => !x.IsAbstract))
            {
                OptionObserverType instance = Activator.CreateInstance(type) as OptionObserverType;
                observers.Add(new ObserverPair()
                {
                    observerType = type,
                    observerName = instance.Name
                });
                instance = null;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Custom Option Observers"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Rect observersLabel = EditorGUILayout.GetControlRect();
                    EditorGUI.LabelField(observersLabel, new GUIContent("Option Observers"), EditorStyles.boldLabel);

                    Rect addObserverRect = observersLabel;
                    addObserverRect.xMin = addObserverRect.xMax - EditorGUIUtility.singleLineHeight;

                    using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                    {
                        using (new EditorDrawing.IconSizeScope(14))
                        {
                            if (GUI.Button(addObserverRect, EditorUtils.Styles.PlusIcon))
                            {
                                Rect dropdownRect = addObserverRect;
                                dropdownRect.width = 250f;
                                dropdownRect.height = 0f;
                                dropdownRect.y += 21f;
                                dropdownRect.x = addObserverRect.x - (250f - EditorGUIUtility.singleLineHeight);

                                ObserversDropdown observersDropdown = new(new(), observers);
                                observersDropdown.OnItemPressed = (type) => AddObserver(type);
                                observersDropdown.Show(dropdownRect);
                            }
                        }
                    }

                    EditorGUILayout.Space(3f);

                    if (Properties["OptionObservers"].arraySize <= 0)
                    {
                        EditorGUILayout.HelpBox("There are currently no option observers. To add a new observer, click the 'Add State' button.", MessageType.Info);
                    }

                    for (int i = 0; i < Properties["OptionObservers"].arraySize; i++)
                    {
                        SerializedProperty observerProperty = Properties["OptionObservers"].GetArrayElementAtIndex(i);
                        PropertyCollection observerProperties = EditorDrawing.GetAllProperties(observerProperty);

                        string observerName = observerProperty.managedReferenceValue.ToString();
                        GUIContent observerHeader = EditorGUIUtility.TrTextContentWithIcon($" {observerName} (Observer)", "Settings");

                        if (EditorDrawing.BeginFoldoutBorderLayout(observerProperty, observerHeader, out Rect observerFoldoutRect))
                        {
                            observerProperties.DrawAll();
                            EditorDrawing.EndBorderHeaderLayout();
                        }

                        Rect optionRemoveButton = observerFoldoutRect;
                        optionRemoveButton.xMin = optionRemoveButton.xMax - EditorGUIUtility.singleLineHeight;
                        optionRemoveButton.y += 3f;

                        if (GUI.Button(optionRemoveButton, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                        {
                            Properties["OptionObservers"].DeleteArrayElementAtIndex(i);
                        }

                        if (i + 1 < Properties["OptionObservers"].arraySize) EditorGUILayout.Space(1f);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddObserver(Type observerType)
        {
            OptionObserverType observer = (OptionObserverType)Activator.CreateInstance(observerType);
            Target.OptionObservers.Add(observer);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        public class ObserversDropdown : AdvancedDropdown
        {
            private readonly IEnumerable<ObserverPair> observers;
            public Action<Type> OnItemPressed;

            private class ObserverElement : AdvancedDropdownItem
            {
                public Type obsevrerType;

                public ObserverElement(string displayName, Type obsevrerType) : base(displayName)
                {
                    this.obsevrerType = obsevrerType;
                }
            }

            public ObserversDropdown(AdvancedDropdownState state, IEnumerable<ObserverPair> observers) : base(state)
            {
                this.observers = observers;
                minimumSize = new Vector2(minimumSize.x, 270f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Option Observers");
                var groupMap = new Dictionary<string, AdvancedDropdownItem>();

                foreach (var observer in observers)
                {
                    Type type = observer.observerType;
                    string name = observer.observerName;

                    // Split the name into groups
                    string[] groups = name.Split('/');

                    // Create or find the groups
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

                    // Create the item and add it to the last group
                    ObserverElement item = new ObserverElement(groups.Last(), type);

                    //item.icon = MotionIcon;
                    parent.AddChild(item);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                ObserverElement element = (ObserverElement)item;
                OnItemPressed?.Invoke(element.obsevrerType);
            }
        }
    }
}