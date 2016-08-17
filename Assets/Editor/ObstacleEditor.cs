using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Obstacle))]
public class ObstacleEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Obstacle obstacle = target as Obstacle;
        BabyRoomGenerator babyRoom = FindObjectOfType<BabyRoomGenerator>().GetComponent<BabyRoomGenerator>();
        if (GUILayout.Button("Regenerate Map"))
        {
            EditorUtility.SetDirty(obstacle.gameObject);
            Object prefab = Resources.Load("Prefabs/BabyCrib");
            babyRoom.furnitures[0] = PrefabUtility.ReplacePrefab(obstacle.gameObject, prefab).GetComponent<Obstacle>();
            babyRoom.GenerateMap();
        }

    }
}
