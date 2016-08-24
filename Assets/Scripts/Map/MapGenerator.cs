using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : Module
{
    public Map map;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    public Transform[] obstaclePrefabs;

    List<Vector3> vertices;
    Vector3[] baseVertices;

    public Vector2 maxMapSize;

    List<Coord> allTileCoords;
    List<Coord> allOpenCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform[,] tileMap;

    [HideInInspector]
    public float tileSize = 2.4f;

    private MeshFilter floorMF;
    private MeshRenderer floorRenderer;

    void Awake()
    {
        if (!transform.FindChild("Generated Map"))
            GenerateMap();
       
    }

    public void GenerateMap()
    {
        GetComponent<BoxCollider>().size = new Vector3(map.mapSize.x * tileSize, .05f, map.mapSize.y * tileSize);

        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
            DestroyImmediate(transform.FindChild(holderName).gameObject);

        Transform mapHolder = new GameObject(holderName).transform;
        floorMF = mapHolder.gameObject.AddComponent<MeshFilter>();
        floorRenderer = mapHolder.gameObject.AddComponent<MeshRenderer>();
        mapHolder.parent = transform;

        GenerateFloor(mapHolder);

        InstantiateNavMask(mapHolder);

        mapHolder.rotation = Quaternion.Euler(0f, (float)map.mapRotation, 0f);

    }

    private void InstantiateNavMask(Transform mapHolder)
    {
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * ((map.mapSize.x + maxMapSize.x) / 4f) * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - map.mapSize.x) / 2f, 1, map.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * ((map.mapSize.x + maxMapSize.x) / 4f) * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - map.mapSize.x) / 2f, 1, map.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * ((map.mapSize.y + maxMapSize.y) / 4f) * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - map.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * ((map.mapSize.y + maxMapSize.y) / 4f) * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - map.mapSize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;//Navmesh floor = MaxMapsize
    }

    private void GenerateFloor(Transform mapHolder)
    {
        Mesh mesh = new Mesh();
        vertices = new List<Vector3>();
        int[] floorTriangles;
        int[] wallTriangles;

        if (map.LRoom)
        {
            GenerateLFloor(out floorTriangles);
            GenerateLWalls(out wallTriangles);
        }
        else
        {
            GenerateQuadFloor(out floorTriangles);
            GenerateQuadWalls(out wallTriangles);
        }

        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetTriangles(floorTriangles, 0);
        mesh.SetTriangles(wallTriangles, 1);
        mesh.RecalculateNormals();
        floorMF.sharedMesh = mesh;
        floorRenderer.materials = new Material[2] { map.floorMaterial, map.wallMaterial };

        MeshCollider meshCol = mapHolder.gameObject.AddComponent<MeshCollider>();
        meshCol.sharedMesh = mesh;
     
    }

    private void GenerateQuadFloor(out int[] triangles)
    {
        triangles = new int[6];

        vertices.Add(CoordToPosition(0, 0));
        vertices.Add(CoordToPosition(map.mapSize.x-1, 0));
        vertices.Add(CoordToPosition(map.mapSize.x-1, map.mapSize.y-1));
        vertices.Add(CoordToPosition(0, map.mapSize.y-1));

        baseVertices = vertices.ToArray();

        CreateQuad(triangles, 0, 0, 1, 3, 2);

    }

    private void GenerateLFloor(out int[] triangles)
    {

        triangles = new int[6 * 4];

        vertices.Add(CoordToPosition(0, 0));
        vertices.Add(CoordToPosition(map.mapSize.x-1, 0));
        vertices.Add(CoordToPosition(map.mapSize.x-1, map.corridorSize.y));
        vertices.Add(CoordToPosition(map.corridorSize.x, map.corridorSize.y));
        vertices.Add(CoordToPosition(map.corridorSize.x, map.mapSize.y-1));
        vertices.Add(CoordToPosition(0, map.mapSize.y-1));

        baseVertices = vertices.ToArray();

        int t = 0;
        t = CreateQuad(triangles, t, 0, 1, 3, 2);
        t = CreateQuad(triangles, t, 0, 3, 5, 4);

    }

    private void GenerateLWalls(out int[] wallTriangles)
    {
        wallTriangles = new int[3 * 2 * 18];
        
        int ring = vertices.Count;

        for (int i = 0; i < ring; i++)
            vertices.Add(vertices[i] + Vector3.up * map.wallHeight);


        Vector3 vec = Quaternion.AngleAxis(45f, vertices[0 + ring] - vertices[0]) * ((vertices[ring - 1] - vertices[0]).normalized * map.wallDepth);
        vertices.Add(vertices[0 + ring] + vec);
        for (int i = 1; i < ring; i++)
        {
            if (vertices[i] == baseVertices[3])
                vec = Quaternion.AngleAxis(135f, vertices[i + ring] - vertices[i]) * ((vertices[i - 1] - vertices[i]).normalized * map.wallDepth);
            else
                vec = Quaternion.AngleAxis(45f, vertices[i + ring] - vertices[i]) * ((vertices[i - 1] - vertices[i]).normalized * map.wallDepth);
            vertices.Add(vertices[i + ring] + vec);
        }

        vec = Quaternion.AngleAxis(45f, vertices[0 + ring] - vertices[0]) * ((vertices[ring - 1] - vertices[0]).normalized * map.wallDepth);
        vertices.Add(vertices[0] + vec);
        for (int i = 1; i < ring; i++)
        {
            if (vertices[i] == baseVertices[3])
                vec = Quaternion.AngleAxis(135f, vertices[i + ring] - vertices[i]) * ((vertices[i - 1] - vertices[i]).normalized * map.wallDepth);
            else
                vec = Quaternion.AngleAxis(45f, vertices[i + ring] - vertices[i]) * ((vertices[i - 1] - vertices[i]).normalized * map.wallDepth);
            vertices.Add(vertices[i] + vec);
        }

        int t = 0;
        for (int i = 0; i < ring - 1; i++)
        {
            t = CreateQuad(wallTriangles, t, i, i + 1, i + ring, i + ring + 1);
            t = CreateQuad(wallTriangles, t, i + ring, i + ring + 1, i + ring * 2, i + ring * 2 + 1);
            t = CreateQuad(wallTriangles, t, i + ring * 3, i + ring * 3 + 1, i + ring * 2, i + ring * 2 + 1, true);
        }
        t = CreateQuad(wallTriangles, t, 5, 0, 5 + ring, ring);
        t = CreateQuad(wallTriangles, t, 5 + ring, ring, 5 + ring * 2, ring * 2);
        t = CreateQuad(wallTriangles, t, 5 + ring * 3, ring * 3, 5 + ring * 2, ring * 2, true);

    }

    private void GenerateQuadWalls(out int[] wallTriangles)
    {
        wallTriangles = new int[3 * 2 * 12];

        int ring = vertices.Count;

        for (int i = 0; i < ring; i++)
            vertices.Add(vertices[i] + Vector3.up * map.wallHeight);

        Vector3 vec = Quaternion.AngleAxis(45f, vertices[0 + ring] - vertices[0]) * ((vertices[ring-1] - vertices[0]).normalized * map.wallDepth);
        vertices.Add(vertices[0 + ring] + vec);
        for (int i = 1; i < ring; i++)
        {
            vec = Quaternion.AngleAxis(45f, vertices[i + ring] - vertices[i]) * ((vertices[i - 1] - vertices[i]).normalized*map.wallDepth);
            vertices.Add(vertices[i + ring] + vec);
        }

        vec = Quaternion.AngleAxis(45f, vertices[0 + ring] - vertices[0]) * ((vertices[ring - 1] - vertices[0]).normalized * map.wallDepth);
        vertices.Add(vertices[0] + vec);
        for (int i = 1; i < ring; i++)
        {
            vec = Quaternion.AngleAxis(45f, vertices[i + ring] - vertices[i]) * ((vertices[i - 1] - vertices[i]).normalized * map.wallDepth);
            vertices.Add(vertices[i] + vec);
        }

        int t = 0;
        for (int i = 0; i < ring-1; i++)
        {
            t = CreateQuad(wallTriangles, t, i, i + 1, i + ring,i + ring + 1);
            t = CreateQuad(wallTriangles, t, i + ring, i + ring + 1, i + ring * 2, i + ring * 2 + 1);
            t = CreateQuad(wallTriangles, t, i + ring * 3, i + ring * 3 + 1, i + ring * 2, i + ring * 2 + 1, true);
        }
        t = CreateQuad(wallTriangles, t, 3, 0, 3 + ring, ring);
        t = CreateQuad(wallTriangles, t, 3 + ring, ring, 3 + ring * 2, ring * 2);
        t = CreateQuad(wallTriangles, t, 3 + ring * 3, ring * 3, 3 + ring * 2, ring * 2, true);
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
        return new Vector3(-map.mapSize.x / 2f + 0.5f + x, 0, -map.mapSize.y / 2f + 0.5f + y) *tileSize;
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }



    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (map.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (map.mapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0)-1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1)-1);

        return tileMap[x, y];
    }

    void OnDrawGizmos()
    {
        if (vertices != null)
        {
            Gizmos.color = Color.black;
            for (int i = 0; i < vertices.Count; i++)
            {
                Gizmos.DrawSphere(vertices[i], 0.1f);
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
    public Material floorMaterial;
    public Material wallMaterial;
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
