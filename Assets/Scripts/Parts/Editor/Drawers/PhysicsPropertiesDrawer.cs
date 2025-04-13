using Parts.Components;
using UnityEditor;
using UnityEngine;

namespace Parts.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PhysicsProperties))]
    public class PhysicsPropertiesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var dragProp = property.FindPropertyRelative("drag");
            var angularDragProp = property.FindPropertyRelative("angularDrag");
            var physicsMaterialProp = property.FindPropertyRelative("physicsMaterial");

            position.height = EditorGUIUtility.singleLineHeight;

            // Draw properties with proper spacing and layout
            EditorGUI.PropertyField(position, dragProp);
            position.y += EditorGUIUtility.singleLineHeight + 2;
            
            EditorGUI.PropertyField(position, angularDragProp);
            position.y += EditorGUIUtility.singleLineHeight + 2;
            
            EditorGUI.PropertyField(position, physicsMaterialProp);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight * 3) + 4;
        }
    }
}