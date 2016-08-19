using UnityEngine;
using System.Collections;

public class SpawnObject : Obstacle {

    public Vector3 spawnPoint { get { return FindObjectOfType<BabyRoomGenerator>().GetComponent<BabyRoomGenerator>().CoordToPosition(spawnTile.x, spawnTile.y) + spawnOffset; } }
}
