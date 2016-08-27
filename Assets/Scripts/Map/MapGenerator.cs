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

    List<Vector3> baseVertices;
    List<Vector3> wallVertices;

    public Vector2 maxMapSize;

    [HideInInspector]
    public float tileSize = 2.4f;

    void Awake()
    {
        if (!transform.FindChild("Doors"))
            GenerateMap();
       
    }

    public void GenerateMap()
    {
        Vector3 startPos = transform.position;
        transform.position = Vector3.zero;

        string doorsHolderName = "Doors";
        if (transform.FindChild(doorsHolderName))
            DestroyImmediate(transform.FindChild(doorsHolderName).gameObject);

        Transform doorsHolder = new GameObject(doorsHolderName).transform;
        doorsHolder.parent = transform;

        GetComponent<BoxCollider>().size = new Vector3(map.mapSize.x * tileSize, .05f, map.mapSize.y * tileSize);

        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
            DestroyImmediate(transform.FindChild(holderName).gameObject);

        GenerateFloor(doorsHolder);

        //InstantiateNavMask();

        transform.rotation = Quaternion.Euler(0f, (float)map.mapRotation, 0f);

        transform.position = startPos;

    }

    private void InstantiateNavMask()
    {
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * ((map.mapSize.x + maxMapSize.x) / 4f) * tileSize, Quaternion.identity) as Transform;
        maskLeft.localScale = new Vector3((maxMapSize.x - map.mapSize.x) / 2f, 1, map.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * ((map.mapSize.x + maxMapSize.x) / 4f) * tileSize, Quaternion.identity) as Transform;
        maskRight.localScale = new Vector3((maxMapSize.x - map.mapSize.x) / 2f, 1, map.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * ((map.mapSize.y + maxMapSize.y) / 4f) * tileSize, Quaternion.identity) as Transform;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - map.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * ((map.mapSize.y + maxMapSize.y) / 4f) * tileSize, Quaternion.identity) as Transform;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - map.mapSize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;//Navmesh floor = MaxMapsize
    }

    private void GenerateFloor(Transform doorsHolder)
    {
        Mesh floorMesh = new Mesh();
        Mesh wallMesh = new Mesh();
        baseVertices = new List<Vector3>();
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

        floorMesh.vertices = baseVertices.ToArray();
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

        baseVertices.Add(CoordToPosition(0, 0));
        baseVertices.Add(CoordToPosition(map.mapSize.x-1, 0));
        baseVertices.Add(CoordToPosition(map.mapSize.x-1, map.mapSize.y-1));
        baseVertices.Add(CoordToPosition(0, map.mapSize.y-1));

        wallVertices = new List<Vector3>(baseVertices);

        CreateQuad(triangles, 0, 0, 1, 3, 2);

    }

    private void GenerateLFloor(out int[] triangles)
    {

        triangles = new int[6 * 4];

        baseVertices.Add(CoordToPosition(0, 0));
        baseVertices.Add(CoordToPosition(map.mapSize.x-1, 0));
        baseVertices.Add(CoordToPosition(map.mapSize.x-1, map.corridorSize.y));
        baseVertices.Add(CoordToPosition(map.corridorSize.x, map.corridorSize.y));
        baseVertices.Add(CoordToPosition(map.corridorSize.x, map.mapSize.y-1));
        baseVertices.Add(CoordToPosition(0, map.mapSize.y-1));

        wallVertices = new List<Vector3>(baseVertices);

        int t = 0;
        t = CreateQuad(triangles, t, 0, 1, 3, 2);
        t = CreateQuad(triangles, t, 0, 3, 5, 4);

    }

    private void GenerateLWalls(out int[] wallTriangles, Transform doorsHolder)
    {
        Door[] doorsReplica = new Door[doors.Length];
        Array.Copy(doors, doorsReplica, doors.Length);
        Array.Sort(doorsReplica);

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

        wallTriangles = new int[6 * 3 * (2 * wallVertices.Count - baseVertices.Count)];

        int ring = wallVertices.Count;

        //Add the ceiling vertices
        for (int i = 0; i < ring; i++)
            wallVertices.Add(wallVertices[i] + Vector3.up * map.wallHeight);

        //Add ceiling vertices matches
        Vector3 vec = Quaternion.AngleAxis(45f, wallVertices[0 + ring] - wallVertices[0]) * ((wallVertices[ring - 1] - wallVertices[0]).normalized * map.wallDepth);
        wallVertices.Add(wallVertices[0 + ring] + vec);
        for (int i = 1; i < ring; i++)
        {
            if (baseVertices.Contains(wallVertices[i]))
                if (wallVertices[i] == baseVertices[3])
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
            if (baseVertices.Contains(wallVertices[i]))
                if (wallVertices[i] == baseVertices[3])
                    vec = Quaternion.AngleAxis(135f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
                else
                    vec = Quaternion.AngleAxis(45f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            else
                vec = Quaternion.AngleAxis(90f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth);
            wallVertices.Add(wallVertices[i] + vec);
        }

        int t = 0;
        int v = 0;
        int doorIndex = 0;
        bool firstDoorVertice = true;
        for (int i = 0; i < ring - 1; i++, v++)
        {
            if (!baseVertices.Contains(wallVertices[i]))
            {
                if (firstDoorVertice)
                {
                    float doorHeight = doorsReplica[doorIndex].doorHeight;

                    wallVertices.Add(wallVertices[i] + Vector3.up * doorHeight);
                    wallVertices.Add(wallVertices[i + 1] + Vector3.up * doorHeight);
                    vec = Quaternion.AngleAxis(90f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth) + Vector3.up * doorHeight;
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

        wallTriangles = new int[6 * 3 * (2*wallVertices.Count - baseVertices.Count)];
        int ring = wallVertices.Count;

        //Add the ceiling vertices
        for (int i = 0; i < ring; i++)
            wallVertices.Add(wallVertices[i] + Vector3.up * map.wallHeight);

        //Add ceiling vertices matches
        Vector3 vec = Quaternion.AngleAxis(45f, wallVertices[0 + ring] - wallVertices[0]) * ((wallVertices[ring-1] - wallVertices[0]).normalized * map.wallDepth);
        wallVertices.Add(wallVertices[0 + ring] + vec);
        for (int i = 1; i < ring; i++)
        {
            if (baseVertices.Contains(wallVertices[i]))
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
            if (baseVertices.Contains(wallVertices[i]))
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
            if (!baseVertices.Contains(wallVertices[i]))
            {
                if (firstDoorVertice)
                {
                    float doorHeight = doorsReplica[doorIndex].doorHeight;

                    wallVertices.Add(wallVertices[i] + Vector3.up * doorHeight);
                    wallVertices.Add(wallVertices[i + 1] + Vector3.up * doorHeight);
                    vec = Quaternion.AngleAxis(90f, wallVertices[i + ring] - wallVertices[i]) * ((wallVertices[i - 1] - wallVertices[i]).normalized * map.wallDepth) + Vector3.up* doorHeight;
                    wallVertices.Add(wallVertices[i] + vec);
                    wallVertices.Add(wallVertices[i + 1] + vec);

                    t = CreateQuad(wallTriangles, t, wallVertices.Count-4, wallVertices.Count-3, i + ring, i + ring + 1);
                    t = CreateQuad(wallTriangles, t, i + ring, i + ring + 1, i + ring * 2, i + ring * 2 + 1);
                    t = CreateQuad(wallTriangles, t, wallVertices.Count-2, wallVertices.Count-1, i + ring * 2, i + ring * 2 + 1, true);

                    Transform doorInstance = Instantiate(doorsReplica[doorIndex].roomDoorPrefab);
                    Transform connection = doorInstance.FindChild("Connector");
                    Vector3 childLocalPos = connection.localPosition;//Store the child to parent position offset
                    connection.GetComponent<Exit>().Room = transform;
                    connection.parent = null;//so the child moves freely
                    connection.position = wallVertices[i] + (wallVertices[i + 1] - wallVertices[i]) / 2 + (wallVertices[i+ring*3]-wallVertices[i])/2;
                    doorInstance.position = connection.position - childLocalPos;
                    connection.parent = doorInstance;
                    doorInstance.forward = Quaternion.AngleAxis(-90f, Vector3.up) * (wallVertices[i + 1] - wallVertices[i]);
                    doorInstance.parent = doorsHolder;
                    doorIndex++;
                    
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
        return transform.position + new Vector3(-map.mapSize.x / 2f + 0.5f + x, 0, -map.mapSize.y / 2f + 0.5f + y) *tileSize;
    }


    void OnDrawGizmos()
    {
        if (wallVertices != null)
        {
            Gizmos.color = Color.black;
            for (int i = 0; i < wallVertices.Count; i++)
            {
                Gizmos.DrawSphere(wallVertices[i], 0.1f);
            }
        }
    }

}

[System.Serializable]
public struct Coord
{
    public float x;
    public float y;

    public Coord(float _x, float _y)
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
    public float obstaclePercent;
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
