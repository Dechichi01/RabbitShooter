using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator : Module
{
    public MeshFilter floorMF;
    public MeshFilter wallMF;

    public Map map;
    public Door[] doors;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    public PlaceableItem[] furnitures;

    List<Vector3> floorVertices;
    List<Vector3> wallVertices;

    public Vector2 maxMapSize;

    List<Coord> allCoords;
    [HideInInspector]
    public List<Coord> openCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Queue<Coord> shuffledTileCoords;

    public int tileSize = 5;

    void Awake()
    {
        if (!transform.FindChild("Generated Map"))
        {
            allCoords = new List<Coord>();
            for (int x = 0; x < map.mapSize.x; x+=tileSize)
                for (int y = 0; y < map.mapSize.y; y+= tileSize)
                    allCoords.Add(new Coord(x, y));

            openCoords = new List<Coord>(allCoords);
        }

        GenerateMap();
    }


    public void GenerateMap()
    {
        if (!Application.isPlaying)
        {
            allCoords = new List<Coord>();
            for (int x = 0; x < map.mapSize.x; x+= tileSize)
                for (int y = 0; y < map.mapSize.y; y+=tileSize)
                    allCoords.Add(new Coord(x, y));

            openCoords = new List<Coord>(allCoords);
        }

        string mapHolderName = "Generated Map";
        if (transform.FindChild(mapHolderName))
            DestroyImmediate(transform.FindChild(mapHolderName).gameObject);

        Transform mapHolder = new GameObject(mapHolderName).transform;
        mapHolder.parent = transform;

        string doorsHolderName = "Doors";
        Transform doorsHolder = new GameObject(doorsHolderName).transform;
        doorsHolder.parent = mapHolder;

        string navMaskHolderName = "NavMask";
        Transform navMaskHolder = new GameObject(navMaskHolderName).transform;
        navMaskHolder.parent = mapHolder;

        string obstacleHolderName = "Obstacles";
        Transform obstacleHolder = new GameObject(obstacleHolderName).transform;
        obstacleHolder.parent = mapHolder;

        //Initiate maxMapSize (used by the NavMesh Mask)
        maxMapSize.x = map.mapSize.x * 1.2f;
        maxMapSize.y = map.mapSize.y * 1.2f;

        GetComponent<BoxCollider>().size = new Vector3(map.mapSize.x , .05f, map.mapSize.y );

        //Store startPos (in case the map is generated in a position different from (0,0,0)
        Vector3 startPos = transform.position;
        transform.position = Vector3.zero;


        GenerateMesh(doorsHolder);

        InstantiateFurniture(obstacleHolder);
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(openCoords.ToArray(), map.seed));
        InstantiateNavMask(navMaskHolder);

        transform.rotation = Quaternion.Euler(0f, (float)map.mapRotation, 0f);

        transform.position = startPos;

    }

    private void InstantiateFurniture(Transform mapHolder)
    {
        for (int i = 0; i < furnitures.Length; i++)
        {
            Vector3 position = CoordToPosition(furnitures[i].spawnTile.x, furnitures[i].spawnTile.y) + furnitures[i].spawnOffset;
            Obstacle furniture = Instantiate(furnitures[i].prefab, position, Quaternion.Euler(furnitures[i].spawnRotation)) as Obstacle;

            furniture.transform.parent = mapHolder;
            furniture.spawnTile = furnitures[i].spawnTile;
            furniture.OccupyTiles(tileSize, ref openCoords);
        }

    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    private void InstantiateNavMask(Transform mapHolder)
    {
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * ((map.mapSize.x + maxMapSize.x) / 4f) , Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - map.mapSize.x) / 2f, 1, map.mapSize.y) ;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * ((map.mapSize.x + maxMapSize.x) / 4f) , Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - map.mapSize.x) / 2f, 1, map.mapSize.y) ;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * ((map.mapSize.y + maxMapSize.y) / 4f) , Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - map.mapSize.y) / 2f) ;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * ((map.mapSize.y + maxMapSize.y) / 4f) , Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - map.mapSize.y) / 2f) ;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) ;//Navmesh floor = MaxMapsize
    }

    private void GenerateMesh(Transform doorsHolder)
    {
        Mesh floorMesh = new Mesh();
        Mesh wallMesh = new Mesh();
        floorVertices = new List<Vector3>();
        wallVertices = new List<Vector3>();
        int[] floorTriangles;
        int[] wallTriangles;

        if (map.LRoom)
        {
            GenerateLFloor(out floorTriangles);
            GenerateLWalls(out wallTriangles, doorsHolder);
        }
        else
        {
            GenerateQuadFloor(out floorTriangles);
            GenerateQuadWalls(out wallTriangles, doorsHolder);
        }

        floorMesh.vertices = floorVertices.ToArray();
        floorMesh.triangles = floorTriangles;
        floorMesh.RecalculateNormals();
        floorMF.sharedMesh = floorMesh;
        floorMF.transform.GetComponent<MeshCollider>().sharedMesh = floorMesh;

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles;
        wallMesh.RecalculateNormals();
        wallMF.sharedMesh = wallMesh;
        wallMF.transform.GetComponent<MeshCollider>().sharedMesh = wallMesh;

    }

    private void GenerateQuadFloor(out int[] triangles)
    {
        triangles = new int[6];

        floorVertices.Add(CoordToPosition(0, 0));
        floorVertices.Add(CoordToPosition(map.mapSize.x-1, 0));
        floorVertices.Add(CoordToPosition(map.mapSize.x-1, map.mapSize.y-1));
        floorVertices.Add(CoordToPosition(0, map.mapSize.y-1));

        wallVertices = new List<Vector3>(floorVertices);

        CreateQuad(triangles, 0, 0, 1, 3, 2);

    }

    private void GenerateLFloor(out int[] triangles)
    {

        triangles = new int[6 * 4];

        floorVertices.Add(CoordToPosition(0, 0));
        floorVertices.Add(CoordToPosition(map.mapSize.x-1, 0));
        floorVertices.Add(CoordToPosition(map.mapSize.x-1, map.corridorSize.y));
        floorVertices.Add(CoordToPosition(map.corridorSize.x, map.corridorSize.y));
        floorVertices.Add(CoordToPosition(map.corridorSize.x, map.mapSize.y-1));
        floorVertices.Add(CoordToPosition(0, map.mapSize.y-1));

        wallVertices = new List<Vector3>(floorVertices);

        int t = 0;
        t = CreateQuad(triangles, t, 0, 1, 3, 2);
        t = CreateQuad(triangles, t, 0, 3, 5, 4);

    }

    private void GenerateLWalls(out int[] wallTriangles, Transform doorsHolder)
    {
        Door[] doorsReplica = new Door[doors.Length];
        Array.Copy(doors, doorsReplica, doors.Length);
        Array.Sort(doorsReplica);

        //Add vertices to represent the doors
        if (doorsReplica.Length > 0)
        {
            for (int i = doorsReplica.Length - 1; i >= 0; i--)
            {
                if (doorsReplica[i].offSetInRoom > 2 * (map.mapSize.x + map.mapSize.y))
                    continue;

                if (doorsReplica[i].offSetInRoom > map.mapSize.x * 2 + map.mapSize.y)
                {
                    wallVertices.Insert(6, CoordToPosition(0, map.mapSize.y - 1 - doorsReplica[i].offSetInRoom + map.mapSize.x * 2 + map.mapSize.y));
                    wallVertices.Insert(7, CoordToPosition(0, map.mapSize.y - 1 - doorsReplica[i].doorWidth - doorsReplica[i].offSetInRoom + map.mapSize.x * 2 + map.mapSize.y));
                }
                else if (doorsReplica[i].offSetInRoom > map.mapSize.x + map.corridorSize.x +  map.mapSize.y)
                {
                    float offSet = doorsReplica[i].offSetInRoom - (map.mapSize.x + map.corridorSize.x + map.mapSize.y);
                    wallVertices.Insert(5, CoordToPosition(map.corridorSize.x - offSet, map.mapSize.y - 1));
                    wallVertices.Insert(6, CoordToPosition(map.corridorSize.x - offSet - doorsReplica[i].doorWidth, map.mapSize.y - 1));
                }
                else if (doorsReplica[i].offSetInRoom > map.mapSize.x + map.corridorSize.y + map.corridorSize.x)
                {
                    float offSet = doorsReplica[i].offSetInRoom - (map.mapSize.x + map.corridorSize.y + map.corridorSize.x);
                    wallVertices.Insert(4, CoordToPosition(map.corridorSize.x, map.corridorSize.y + offSet));
                    wallVertices.Insert(5, CoordToPosition(map.corridorSize.x, map.corridorSize.y + offSet + doorsReplica[i].doorWidth));
                }
                else if (doorsReplica[i].offSetInRoom > map.mapSize.x + map.corridorSize.y)
                {
                    float offSet = doorsReplica[i].offSetInRoom - map.mapSize.x - map.corridorSize.y;
                    wallVertices.Insert(3, CoordToPosition(map.mapSize.x - 1 - offSet, map.corridorSize.y));
                    wallVertices.Insert(4, CoordToPosition(map.mapSize.x - 1  - offSet - doorsReplica[i].doorWidth, map.corridorSize.y));
                }
                else if (doorsReplica[i].offSetInRoom > map.mapSize.x)
                {
                    float offSet = doorsReplica[i].offSetInRoom - map.mapSize.x;
                    wallVertices.Insert(2, CoordToPosition(map.mapSize.x - 1, offSet));
                    wallVertices.Insert(3, CoordToPosition(map.mapSize.x - 1, offSet + doorsReplica[i].doorWidth));
                }
                else if (doorsReplica[i].offSetInRoom > 0)
                {
                    wallVertices.Insert(1, CoordToPosition(doorsReplica[i].offSetInRoom, 0));
                    wallVertices.Insert(2, CoordToPosition(doorsReplica[i].offSetInRoom + doorsReplica[i].doorWidth, 0));
                }
            }
        }
        //end

        wallTriangles = new int[6 * 3 * (2 * wallVertices.Count - floorVertices.Count)];
        int ring = wallVertices.Count;

        //Add the ceiling vertices
        for (int i = 0; i < ring; i++)
            wallVertices.Add(wallVertices[i] + Vector3.up * map.wallHeight);

        //Add ceiling vertices matches
        Vector3 vec = Quaternion.AngleAxis(45f, wallVertices[0 + ring] - wallVertices[0]) * ((wallVertices[ring - 1] - wallVertices[0]).normalized * map.wallDepth);
        wallVertices.Add(wallVertices[0 + ring] + vec);
        for (int i = 1; i < ring; i++)
        {
            if (floorVertices.Contains(wallVertices[i]))
                if (wallVertices[i] == floorVertices[3])
                    vec = Quaternion.AngleAxis(135f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
                else
                    vec = Quaternion.AngleAxis(45f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            else
                vec = Quaternion.AngleAxis(90f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            wallVertices.Add(wallVertices[i + ring] + vec);
        }

        //Add base vertices matches
        vec = Quaternion.AngleAxis(45f, wallVertices[0 + ring] - wallVertices[0]) * ((wallVertices[ring - 1] - wallVertices[0]).normalized * map.wallDepth);
        wallVertices.Add(wallVertices[0] + vec);
        for (int i = 1; i < ring; i++)
        {
            if (floorVertices.Contains(wallVertices[i]))
                if (wallVertices[i] == floorVertices[3])
                    vec = Quaternion.AngleAxis(135f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
                else
                    vec = Quaternion.AngleAxis(45f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            else
                vec = Quaternion.AngleAxis(90f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            wallVertices.Add(wallVertices[i] + vec);
        }

        //Add the triangles
        int t = 0;
        int v = 0;
        int doorIndex = 0;
        bool firstDoorVertice = true;
        for (int i = 0; i < ring - 1; i++, v++)
        {
            if (!floorVertices.Contains(wallVertices[i]))
            {
                if (firstDoorVertice)
                {
                    SpawnDoor(wallTriangles, doorsHolder, doorsReplica, ring, ref t, ref doorIndex, i);
                }

                firstDoorVertice = !firstDoorVertice;
            }

            if (firstDoorVertice)
            {
                t = CreateQuad(wallTriangles, t, i, i + 1, i + ring, i + ring + 1);
                t = CreateQuad(wallTriangles, t, i + ring, i + ring + 1, i + ring * 2, i + ring * 2 + 1);
                t = CreateQuad(wallTriangles, t, i + ring * 3, i + ring * 3 + 1, i + ring * 2, i + ring * 2 + 1, true);
            }
        }
        t = CreateQuad(wallTriangles, t, v, 0, v + ring, ring);
        t = CreateQuad(wallTriangles, t, v + ring, ring, v + ring * 2, ring * 2);
        t = CreateQuad(wallTriangles, t, v + ring * 3, ring * 3, v + ring * 2, ring * 2, true);

    }

    private void GenerateQuadWalls(out int[] wallTriangles, Transform doorsHolder)
    {

        Door[] doorsReplica = new Door[doors.Length];
        Array.Copy(doors, doorsReplica, doors.Length);
        Array.Sort(doorsReplica);
        //Add the door vertices
        if (doorsReplica.Length > 0)
        {
            for (int i = doorsReplica.Length-1; i >=0; i--)
            {
                if (doorsReplica[i].offSetInRoom > 2 * (map.mapSize.x + map.mapSize.y))
                    continue;

                if (doorsReplica[i].offSetInRoom > map.mapSize.x * 2 + map.mapSize.y)
                {
                    float offSet = doorsReplica[i].offSetInRoom - (map.mapSize.x * 2 + map.mapSize.y);
                    wallVertices.Insert(4, CoordToPosition(0, map.mapSize.y -1 - offSet));
                    wallVertices.Insert(5, CoordToPosition(0, map.mapSize.y - 1 -offSet - doorsReplica[i].doorWidth));
                }
                else if (doorsReplica[i].offSetInRoom > map.mapSize.x + map.mapSize.y)
                {
                    float offSet = doorsReplica[i].offSetInRoom - (map.mapSize.x + map.mapSize.y);
                    wallVertices.Insert(3, CoordToPosition(map.mapSize.x - 1 - offSet, map.mapSize.y-1 ));
                    wallVertices.Insert(4, CoordToPosition(map.mapSize.x - 1 - offSet - doorsReplica[i].doorWidth, map.mapSize.y - 1));
                }
                else if (doorsReplica[i].offSetInRoom > map.mapSize.x)
                {
                    float offSet = doorsReplica[i].offSetInRoom - map.mapSize.x;
                    wallVertices.Insert(2, CoordToPosition(map.mapSize.x-1, offSet));
                    wallVertices.Insert(3, CoordToPosition(map.mapSize.x-1, offSet + doorsReplica[i].doorWidth));
                }
                else if (doorsReplica[i].offSetInRoom >0)
                {
                    wallVertices.Insert(1,CoordToPosition(doorsReplica[i].offSetInRoom,0));
                    wallVertices.Insert(2, CoordToPosition(doorsReplica[i].offSetInRoom + doorsReplica[i].doorWidth, 0));
                }
            }
        }

        wallTriangles = new int[6 * 3 * (2*wallVertices.Count - floorVertices.Count)];
        int ring = wallVertices.Count;

        //Add the ceiling vertices
        for (int i = 0; i < ring; i++)
            wallVertices.Add(wallVertices[i] + Vector3.up * map.wallHeight);

        //Add ceiling vertices matches
        Vector3 vec = Quaternion.AngleAxis(45f, wallVertices[0 + ring] - wallVertices[0]) * ((wallVertices[ring-1] - wallVertices[0]).normalized * map.wallDepth);
        wallVertices.Add(wallVertices[0 + ring] + vec);
        for (int i = 1; i < ring; i++)
        {
            if (floorVertices.Contains(wallVertices[i]))
                vec = Quaternion.AngleAxis(45f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            else
                vec = Quaternion.AngleAxis(90f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            wallVertices.Add(wallVertices[i + ring] + vec);
        }

        //Add base vertices matches
        vec = Quaternion.AngleAxis(45f, wallVertices[0 + ring] - wallVertices[0]) * ((wallVertices[ring - 1] - wallVertices[0]).normalized * map.wallDepth);
        wallVertices.Add(wallVertices[0] + vec);
        for (int i = 1; i < ring; i++)
        {
            if (floorVertices.Contains(wallVertices[i]))
                vec = Quaternion.AngleAxis(45f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            else
                vec = Quaternion.AngleAxis(90f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            wallVertices.Add(wallVertices[i] + vec);
        }

        //Create the triangles
        int t = 0;
        int v = 0;
        int doorIndex = 0;
        bool firstDoorVertice = true;
        for (int i = 0; i < ring - 1; i++, v++)
        {
            if (!floorVertices.Contains(wallVertices[i]))
            {
                if (firstDoorVertice)
                {
                    SpawnDoor(wallTriangles, doorsHolder, doorsReplica, ring, ref t, ref doorIndex, i);
                }
                firstDoorVertice = !firstDoorVertice;
            }

            if (firstDoorVertice)
            {
                t = CreateQuad(wallTriangles, t, i, i + 1, i + ring, i + ring + 1);
                t = CreateQuad(wallTriangles, t, i + ring, i + ring + 1, i + ring * 2, i + ring * 2 + 1);
                t = CreateQuad(wallTriangles, t, i + ring * 3, i + ring * 3 + 1, i + ring * 2, i + ring * 2 + 1, true);
            }
        }
        t = CreateQuad(wallTriangles, t, v, 0, v + ring, ring);
        t = CreateQuad(wallTriangles, t, v + ring, ring, v + ring * 2, ring * 2);
        t = CreateQuad(wallTriangles, t, v + ring * 3, ring * 3, v + ring * 2, ring * 2, true);
    }

    private int CreateQuad(int[] triangles, int i, int v00, int v01, int v10, int v11, bool inverted = false)
    {
        triangles[i] = v00;
        triangles[i+1] = triangles[i+4] = (inverted)?v01:v10;
        triangles[i+2] = triangles[i+3] = (inverted)?v10:v01;
        triangles[i+5] = v11;

        return i + 6;
    }


    void OnNewWave(int waveNumber)
    {
        GenerateMap();
    }

    public Vector3 CoordToPosition(float x, float y)
    {
        return transform.position + new Vector3(-map.mapSize.x / 2f + 0.5f + x, 0, -map.mapSize.y / 2f + 0.5f + y);
    }

    public Coord GetTileFromPosition(Vector3 position)
    {
        /*int x = Mathf.RoundToInt(position.x / + (map.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / + (map.mapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, map.mapSize.x - 1);
        y = Mathf.Clamp(y, 0, map.mapSize.y - 1);*/

        int x = (int) (position.x + map.mapSize.x / 2f - 0.5f - transform.position.x);
        int y = (int) (position.z + map.mapSize.y / 2f - 0.5f - transform.position.z);

        return new Coord(x, y);
    }

    private void SpawnDoor(int[] wallTriangles, Transform doorsHolder, Door[] doorsReplica, int ring, ref int t, ref int doorIndex, int i)
    {
        float doorHeight = doorsReplica[doorIndex].doorHeight;

        wallVertices.Add(wallVertices[i] + Vector3.up * doorHeight);
        wallVertices.Add(wallVertices[i + 1] + Vector3.up * doorHeight);
        Vector3 vec = Quaternion.AngleAxis(90f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth) + Vector3.up * doorHeight;
        wallVertices.Add(wallVertices[i] + vec);
        wallVertices.Add(wallVertices[i + 1] + vec);

        t = CreateQuad(wallTriangles, t, wallVertices.Count - 4, wallVertices.Count - 3, i + ring, i + ring + 1);
        t = CreateQuad(wallTriangles, t, i + ring, i + ring + 1, i + ring * 2, i + ring * 2 + 1);
        t = CreateQuad(wallTriangles, t, wallVertices.Count - 2, wallVertices.Count - 1, i + ring * 2, i + ring * 2 + 1, true);

        Transform doorInstance = Instantiate(doorsReplica[doorIndex].roomDoorPrefab);
        Transform connection = doorInstance.FindChild("Connector");
        Vector3 childLocalPos = connection.localPosition;//Store the child to parent position offset
        connection.GetComponent<Exit>().Room = transform;
        connection.parent = null;//so the child moves freely
        connection.position = wallVertices[i] + (wallVertices[i + 1] - wallVertices[i]) / 2 + (wallVertices[i + ring * 3] - wallVertices[i]) / 2;
        doorInstance.position = connection.position - childLocalPos;
        connection.parent = doorInstance;
        doorInstance.forward = Quaternion.AngleAxis(-90f, Vector3.up) * (wallVertices[i + 1] - wallVertices[i]);
        doorInstance.parent = doorsHolder;
        doorIndex++;
    }


    void OnDrawGizmos()
    {
        for (int i = 0; i < openCoords.Count; i++)
        {
            Gizmos.DrawCube(CoordToPosition(openCoords[i].x, openCoords[i].y), Vector3.one/2);
        }
    }

}

[System.Serializable]
public struct Coord
{
    public int x;
    public int y;

    public Coord(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public static bool operator ==(Coord c1, Coord c2)
    {
        return c1.x == c2.x && c1.y == c2.y;
    }

    public static bool operator !=(Coord c1, Coord c2)
    {
        return !(c1 == c2);
    }

}

[System.Serializable]
public class Map
{
    public enum MapRotations {
        N = 0,
        S = 180,
        W = 90,
        E = 270
    };
    public Coord mapSize;
    public MapRotations mapRotation;
    public Coord corridorSize;
    public bool LRoom;

    public float wallDepth = 0.5f;
    public float wallHeight = 3.2f;
    [Range(0, 1f)]
    public int seed;

    public Coord mapCentre
    {
        get
        {
            return new Coord(mapSize.x / 2, mapSize.y / 2);
        }
    }
}

[System.Serializable]
public class Door : IComparable
{
    public Transform roomDoorPrefab;
    public float doorHeight;
    public float doorWidth;
    public float offSetInRoom;

    public int CompareTo(object obj)
    {
        Door otherDoor = (Door)obj;
        if (otherDoor !=null)
            return (int)(offSetInRoom - otherDoor.offSetInRoom);
        else
            throw new ArgumentException("Object is not a Door");
    }
}

[System.Serializable]
public class PlaceableItem
{
    public Obstacle prefab;
    public Coord spawnTile;
    public Vector3 spawnOffset;
    public Vector3 spawnRotation;
}
