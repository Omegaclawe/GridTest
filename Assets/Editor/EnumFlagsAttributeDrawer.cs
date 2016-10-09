// ---
// This is EnumFlagsAttributeDrawer.cs
// This file needs to be placed in an "Editor" directory.
// ---
using UnityEngine;
using UnityEditor;
 
namespace CodingJar
{
    /// <summary>
    /// This allows us to use an Enum as a masking field (just like the LayerMask in Unity).
    /// </summary>
 
    /// <example>
    ///
    /// // First define the Enumeration type and follow .NET-friendly practice of specifying it with System.FlagsAttribute.
    /// [System.Flags]
    /// enum MyFlagType
    /// {
    ///     Zero = 0,
    ///     One = (1 << 0),
    ///     Two = (1 << 1),
    ///     Three = (1 << 2)
    /// }
    ///
    /// // Now define the actual field, and tag on the EnumFlagsFieldAttribute.
    /// [SerializeField, EnumFlagsField] MyFlagType     _fieldName;
    ///
    /// </example>
 
        [CustomPropertyDrawer(typeof(EnumFlagsFieldAttribute))]
        class EnumFlagsAttributeDrawer : PropertyDrawer
        {
                private System.Type             _objectType;
 
                /// <summary>
                /// Override OnGUI to show a maskable field.
                /// </summary>
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent content)
                {
            // We use a bunch of reflection tricks to try and ascertain the type of the passed-in SerializedProperty.
                        var propertyField = property.serializedObject.targetObject.GetType().GetField( property.name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
                        if ( propertyField != null )
                                _objectType = propertyField.FieldType;
 
            // Help us out if we messed up
                        if ( property.propertyType != SerializedPropertyType.Enum )
                        {
                EditorGUI.HelpBox( position, "System.Flags is only valid on Enumeration types", MessageType.Error );
                        }
            else if ( _objectType == null )
                        {
                                EditorGUI.HelpBox( position, "Could not deduce Enumeration Type", MessageType.Error );
                        }
                        else
                        {
                // Just add-on some text so we know we're working...
                content.text += " (Flags)";
                var valueObj = System.Enum.ToObject( _objectType, property.intValue );
                var newValue = EditorGUI.EnumMaskField( position, content, valueObj as System.Enum );
                if ( GUI.changed )
                {
                    property.intValue = newValue.GetHashCode();
                }
                        }
                }
        }
}