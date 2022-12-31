using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var map = (Map)target;

        if (GUILayout.Button("Generate"))
        {
            map.Generate();
        }
    }
}
