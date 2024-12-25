// Adapted from: https://discussions.unity.com/t/editor-tool-better-scriptableobject-inspector-editing/671671/88
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
#endif

#if UNITY_EDITOR
/// <summary>
/// Draws the property field for any field marked with Expandable.
/// Note: Does not automatically update displayed expanded attribute when the property is changed in the inspector.
/// </summary>
[CustomPropertyDrawer(typeof(ExpandableAttribute), true)]
public class ExpandableAttributeDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new();
        
        PropertyField propertyField = new PropertyField(property);

        container.Add(propertyField);

        if (property.objectReferenceValue == null) return container;
        
        SerializedObject targetObject = new(property.objectReferenceValue);
        if (targetObject == null) return container;
        
        Foldout foldout = new () { text = $"Expand {property.displayName}" };
        container.Add(foldout);
        
        int index = 0;
        SerializedProperty field = targetObject.GetIterator();
        field.NextVisible(true);
        while (field.NextVisible(false)) 
        {
            PropertyField next = null;
            try
            {
                next = new(field);
                next.Bind(field.serializedObject);
            }
            catch (StackOverflowException)
            {
                field.objectReferenceValue = null;
                Debug.LogError("Detected self-nesting causing a StackOverflowException, avoid using the same object inside a nested structure.");
            }
            catch (Exception e) 
            {
                Debug.Log(e);
            }
            
            if (next != null) foldout.Add(next);

            index++;
        }

        targetObject.ApplyModifiedProperties();

        return container;
    }
}
#endif