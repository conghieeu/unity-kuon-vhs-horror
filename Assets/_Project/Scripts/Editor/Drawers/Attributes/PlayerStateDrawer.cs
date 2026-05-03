using System.Linq;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Attributes;

namespace ThunderWire.Editors
{
    [CustomPropertyDrawer(typeof(PlayerStatePickerAttribute))]
    public class PlayerStateDrawer : PropertyDrawer
    {
        private const string Default = MotionBlender.Default;
        private readonly string[] avaiableStates;

        public PlayerStateDrawer()
        {
            var types = TypeCache.GetTypesDerivedFrom<PlayerStateAsset>().Where(x => !x.IsAbstract);
            avaiableStates = new string[types.Count()];
            int index = 0;

            foreach (var type in types)
            {
                PlayerStateAsset stateAsset = (PlayerStateAsset)ScriptableObject.CreateInstance(type);
                avaiableStates[index++] = stateAsset.StateKey;
                Object.DestroyImmediate(stateAsset);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PlayerStatePickerAttribute att = attribute as PlayerStatePickerAttribute;

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "PlayerState Attribute can only be used at string type fields!");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            {
                string[] states = avaiableStates;
                if (att.includeDefault) states = new string[] { Default }.Concat(avaiableStates).ToArray();

                GUIContent title = new(Default);
                string selected = property.stringValue;

                if (string.IsNullOrEmpty(property.stringValue))
                    title = new("None");
                else if (!states.Contains(selected)) 
                    selected = "Missing State";

                Rect popupRect = EditorGUI.PrefixLabel(position, label);
                EditorDrawing.DrawStringSelectPopup(popupRect, title, states, selected, (state) =>
                {
                    property.stringValue = state;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            EditorGUI.EndProperty();
        }
    }
}