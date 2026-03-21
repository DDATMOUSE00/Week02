#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingLayerGenerator))]

public class BuildingLayerGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10f);

        BuildingLayerGenerator generator = (BuildingLayerGenerator)target;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Generation Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate All"))
        {
            generator.GenerateAll();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Generate Far Layer"))
        {
            generator.GenerateFarLayer();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Generate Front Layer"))
        {
            generator.GenerateFrontLayer();
            EditorUtility.SetDirty(generator);
        }

        GUILayout.Space(5f);

        if (GUILayout.Button("Clear All"))
        {
            generator.ClearAll();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear Far Layer"))
        {
            generator.ClearFarLayer();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear Front Layer"))
        {
            generator.ClearFrontLayer();
            EditorUtility.SetDirty(generator);
        }

        EditorGUILayout.EndVertical();
    }
}
#endif