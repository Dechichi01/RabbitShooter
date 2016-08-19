using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour {

    public Coord spawnTile;
    public Vector3 spawnOffset;
    public Vector3 spawnRotation;
    public int xTilesToOccupy, ytilesToOccupy;

    public void OccupyTiles(ref List<Coord> allOpenCoords)
    {
        for (int x = spawnTile.x - xTilesToOccupy; x < spawnTile.x + xTilesToOccupy; x++)
            for (int y = spawnTile.y - ytilesToOccupy; y < spawnTile.y + ytilesToOccupy; y++)
                allOpenCoords.Remove(new Coord(x, y));
    }

}
