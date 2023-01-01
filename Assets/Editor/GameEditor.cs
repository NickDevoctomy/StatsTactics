using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Game))]
public class GameEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var game = (Game)target;

        if (GUILayout.Button("Initialise"))
        {
            game.Initialise();
        }
    }
}
