using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (BoxCollider))]
[RequireComponent (typeof (NavMeshObstacle))]
public class Obstacle : LivingEntity {

    public Coord spawnTile;
    public Vector3 spawnOffset;
    public Vector3 spawnRotation;
    public int xTilesToOccupy, yTilesToOccupy;

    [HideInInspector]
    public List<Coord> occupiedTiles;

    void OnEnable()
    {
        Debug.Log("Ok");
        MapGenerator room = FindObjectOfType<MapGenerator>().GetComponent<MapGenerator>();
        OccupyTiles(ref room.openCoords);
    }

    public void OccupyTiles(ref List<Coord> allOpenCoords)
    {
        occupiedTiles = new List<Coord>();

        for (int x = spawnTile.x - xTilesToOccupy + 1; x < spawnTile.x + xTilesToOccupy; x++)
            for (int y = spawnTile.y - yTilesToOccupy + 1; y < spawnTile.y + yTilesToOccupy; y++)
            {
                Coord coord = new Coord(x, y);
                allOpenCoords.Remove(coord);
                occupiedTiles.Add(coord);
            }
    }

    void OnDrawGizmosSelected()
    {
        MapGenerator room = FindObjectOfType<MapGenerator>().GetComponent<MapGenerator>();

        Gizmos.color = Color.yellow;
        for (int i = 0; i < occupiedTiles.Count; i++)
        {
            Gizmos.DrawCube(room.CoordToPosition(occupiedTiles[i].x, occupiedTiles[i].y), Vector3.one);
        }
    }

}
