using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Obstacle), true)]
public class ObstacleEditor : Editor
{

    public override void OnInspectorGUI()
    {
        Obstacle obstacle = (Obstacle)target;
        MapGenerator room = FindObjectOfType<MapGenerator>().GetComponent<MapGenerator>();

        if (DrawDefaultInspector())
        {
            List<Coord> lol = new List<Coord>();
            obstacle.OccupyTiles(ref lol);
        }

        if (GUILayout.Button("Apply To Prefabs"))
        {
            string prefabName = obstacle.gameObject.name.Replace("(Clone)", "");
            Object prefab = Resources.Load("Prefabs/" + prefabName);

            for (int i = 0; i < room.furnitures.Length; i++)
                if (room.furnitures[i].prefab.gameObject == prefab)
                    room.furnitures[i].prefab = PrefabUtility.ReplacePrefab(obstacle.gameObject, prefab).GetComponent<Obstacle>();

            room.GenerateMap();
        }
    }

}
