using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(CameraController))]
public class CameraEditor : Editor {

    public override void OnInspectorGUI()
    {
        CameraController cam = target as CameraController;

        if (DrawDefaultInspector())
            cam.SinchronizePosition();
    }
}
