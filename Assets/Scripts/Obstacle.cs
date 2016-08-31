using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (BoxCollider))]
[RequireComponent (typeof (NavMeshObstacle))]
public class Obstacle : LivingEntity {

    [HideInInspector]
    public Coord spawnTile;
    public int xTilesToOccupy, yTilesToOccupy;

    [HideInInspector]
    public List<Coord> occupiedTiles;

    void OnEnable()
    {
        MapGenerator room = FindObjectOfType<MapGenerator>().GetComponent<MapGenerator>();
        OccupyTiles(ref room.openCoords);
    }

    public void OccupyTiles(ref List<Coord> allOpenCoords)
    {
        spawnTile = FindObjectOfType<MapGenerator>().GetComponent<MapGenerator>().GetTileFromPosition(transform.position);
        occupiedTiles = new List<Coord>();
        float yRot = transform.rotation.eulerAngles.y;
        yRot = (yRot <= 180) ? yRot : yRot - 180;

        int tilesOnX = (yRot > 50f && yRot < 140) ? yTilesToOccupy : xTilesToOccupy;
        int tilesOnY = (yRot > 50f && yRot < 140) ? xTilesToOccupy : yTilesToOccupy;

        for (int x = spawnTile.x - tilesOnX+1; x < spawnTile.x + tilesOnX; x++)
            for (int y = spawnTile.y - tilesOnY+1; y < spawnTile.y + tilesOnY; y++)
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
            //Vector3 pos = transform.position - Vector3.right*occupiedTiles[i].x - Vector3.forward*occupiedTiles[i].y + room.CoordToPosition(spawnTile.x, spawnTile.y);
            //Gizmos.DrawCube(pos, Vector3.one);
            Gizmos.DrawCube(room.CoordToPosition(occupiedTiles[i].x, occupiedTiles[i].y), Vector3.one*room.tileSize);
        }
    }

}
