using UnityEditor;
using UnityEngine;

public class VehicleLightShaderGUI : ShaderGUI // Must inherit from ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // Main Body Section
        EditorGUILayout.LabelField("Body Settings", EditorStyles.boldLabel);
        MaterialProperty bodyColor = FindProperty("_BodyColor", properties);
        MaterialProperty bodyTex = FindProperty("_BodyTex", properties);
        materialEditor.ShaderProperty(bodyColor, bodyColor.displayName);
        materialEditor.ShaderProperty(bodyTex, bodyTex.displayName);

        // Headlights Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Headlights", EditorStyles.boldLabel);
        MaterialProperty headlightColor = FindProperty("_HeadlightColor", properties);
        materialEditor.ShaderProperty(headlightColor, headlightColor.displayName);

        // Show remaining properties
        base.OnGUI(materialEditor, properties);
    }
}
