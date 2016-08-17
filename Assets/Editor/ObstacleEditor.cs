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
            string prefabName = obstacle.gameObject.name.Replace("(Clone)", "");
            Object prefab = Resources.Load("Prefabs/" + prefabName);

            for (int i = 0; i < babyRoom.furnitures.Length; i++)
                if (babyRoom.furnitures[i].gameObject == prefab)
                    babyRoom.furnitures[i] = PrefabUtility.ReplacePrefab(obstacle.gameObject, prefab).GetComponent<Obstacle>();

            babyRoom.GenerateMap();
        }

    }
}
