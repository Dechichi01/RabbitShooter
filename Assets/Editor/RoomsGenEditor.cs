using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RoomsGenerator))]
public class RoomsGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        RoomsGenerator roomGen = target as RoomsGenerator;
        if (DrawDefaultInspector() || GUILayout.Button("Generate"))
            roomGen.GenerateRooms();

    }
}

