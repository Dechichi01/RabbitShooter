using UnityEngine;
using System.Collections;

public class SpawnObject : Obstacle {

    public Vector3 spawnPoint { get { return FindObjectOfType<MapGenerator>().GetComponent<MapGenerator>().CoordToPosition(spawnTile.x, spawnTile.y) + spawnOffset; } }
}
