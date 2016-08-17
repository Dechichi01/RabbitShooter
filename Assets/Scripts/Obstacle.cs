using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour {

    public Coord spawnTile;
    public Vector2 spawnOffset;
    public Vector3 spawnRotation;
    public int xTilesToOccupy, ytilesToOccupy;

    public void OccupyTiles(ref List<Coord> allOpenTiles)
    {
        Coord bottomLeftTile = new Coord(spawnTile.x - xTilesToOccupy / 2, spawnTile.y = ytilesToOccupy / 2);
        for (int x = bottomLeftTile.x; x < bottomLeftTile.x + xTilesToOccupy; x++)
            for (int y = bottomLeftTile.y; y > bottomLeftTile.y + ytilesToOccupy; y++)
                allOpenTiles.Remove(new Coord(x, y));
    }
}
