using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor {

    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = target as MapGenerator;
        if (DrawDefaultInspector() || GUILayout.Button("Generate"))
            mapGen.GenerateMap();
    }
}
