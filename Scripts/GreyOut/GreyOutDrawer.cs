using UnityEngine;
using Attributes;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(GreyOut))]
public class GreyOutDrawer : PropertyDrawer {
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    return EditorGUI.GetPropertyHeight(property, true);
  }
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    GUI.enabled = false;
    EditorGUI.PropertyField(position, property, label);
    GUI.enabled = true;
  }
}

#endif