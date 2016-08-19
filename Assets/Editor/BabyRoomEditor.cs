using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof(BabyRoomGenerator))]
public class BabyRoomEditor : Editor {

    public override void OnInspectorGUI()
    {
        BabyRoomGenerator map = target as BabyRoomGenerator;

        if (DrawDefaultInspector())
            map.GenerateMap();

        if (GUILayout.Button("Generate Map"))
            map.GenerateMap();
                
    }
}
