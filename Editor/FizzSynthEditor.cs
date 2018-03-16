using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [CustomPropertyDrawer(typeof(Signal))]
    public class SignalDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProperty = property.FindPropertyRelative("id");
            var valueProperty = property.FindPropertyRelative("localValue");
            var attrs = fieldInfo.GetCustomAttributes(typeof(SignalRangeAttribute), true);
            if (attrs.Length > 0)
            {
                var attr = attrs[0] as SignalRangeAttribute;
                position = EditorGUI.PrefixLabel(position, label);
                valueProperty.floatValue = EditorGUI.Slider(position, valueProperty.floatValue, attr.min, attr.max);
            }
            else
                EditorGUI.PropertyField(position, valueProperty, label);
        }

    }

}