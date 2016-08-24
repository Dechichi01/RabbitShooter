using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : Module
{
    public Map map;
    public RoomDoor doorPrefab;
    [Range (1,4)]
    public int doorQuantity;
    public Obstacle[] furnitures;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    public Transform[] obstaclePrefabs;

    List<Vector3> vertices;

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
        
        /*
        Spawner spawner = FindObjectOfType<Spawner>();
        if (spawner!= null)
            FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
        else
            GenerateMap();*/
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
        //InstantiateWalls(mapHolder);
        InstantiateFurniture(mapHolder);


        InstantiateObstacles(mapHolder);
        InstantiateNavMask(mapHolder);

        mapHolder.rotation = Quaternion.Euler(0f, (float)map.mapRotation, 0f);

    }

    private void InstantiateFurniture(Transform mapHolder)
    {
        for (int i = 0; i < furnitures.Length; i++)
        {
            Vector3 position = CoordToPosition(furnitures[i].spawnTile.x, furnitures[i].spawnTile.y) + furnitures[i].spawnOffset;
            Obstacle furniture = Instantiate(furnitures[i], position, Quaternion.Euler(furnitures[i].spawnRotation)) as Obstacle;

            furniture.transform.parent = mapHolder;
            furniture.OccupyTiles(ref allOpenCoords);
        }
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


    private void InstantiateObstacles(Transform mapHolder)
    {
        System.Random prng = new System.Random(map.seed);

        bool[,] obstacleMap = new bool[(int)map.mapSize.x, (int)map.mapSize.y];
        //allOpenCoords = new List<Coord>(allTileCoords);

        int obstacleCount = (int)(map.mapSize.x * map.mapSize.y * map.obstaclePercent);
        int currentObstacleCount = 0;

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != map.mapCentre /*&& MapIsFullyAccessible(obstacleMap, currentObstacleCount)*/)
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                //Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.Euler(0f, Random.Range(0,360), 0f)) as Transform;
                Transform newObstacle = Instantiate(obstaclePrefabs[Random.Range(0,obstaclePrefabs.Length)], obstaclePosition + Vector3.up*0.25f, Quaternion.Euler(0f, Random.Range(0,360), 0f)) as Transform;
                //newObstacle.localScale = new Vector3(tileSize, tileSize, tileSize);
                newObstacle.parent = mapHolder;//So it gets destroyed with the rest of the map

                allOpenCoords.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
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
        vertices.Add(CoordToPosition(map.mapSize.x, 0));
        vertices.Add(CoordToPosition(map.mapSize.x, map.mapSize.y));
        vertices.Add(CoordToPosition(0, map.mapSize.y));

        CreateQuad(triangles, 0, 0, 1, 3, 2);

    }

    private void GenerateLFloor(out int[] triangles)
    {

        triangles = new int[6 * 4];

        int corridorX = (map.corridorSize.x > 0) ? map.corridorSize.x : (map.mapSize.x + map.corridorSize.x);
        int corridorY = (map.corridorSize.y > 0) ? map.corridorSize.y : (map.mapSize.y + map.corridorSize.y);

        vertices.Add(CoordToPosition(0, 0));
        vertices.Add(CoordToPosition(map.mapSize.x, 0));
        vertices.Add(CoordToPosition(map.mapSize.x, map.corridorSize.y));
        vertices.Add(CoordToPosition(map.corridorSize.x, map.corridorSize.y));
        vertices.Add(CoordToPosition(map.corridorSize.x, map.mapSize.y));
        vertices.Add(CoordToPosition(0, map.mapSize.y));

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

        Vector3 centerPoint = vertices[0] + (vertices[2] - vertices[0]) / 2 + Vector3.up * map.wallHeight; ;
        for (int i = 0; i < ring; i++)
        {
            Vector3 currentCP;
            if (i == 3)
                currentCP = vertices[0] + (vertices[3] - vertices[0]) / 2 + Vector3.up*map.wallHeight;
            else if (i == 4)
                currentCP = vertices[3] + (vertices[5] - vertices[3]) / 2 + Vector3.up*map.wallHeight;
            else
                currentCP = centerPoint;

            vertices.Add(vertices[i + ring] + (currentCP - vertices[i + ring]).normalized * map.wallDepth);
        }

        centerPoint = vertices[0] + (vertices[2] - vertices[0]) / 2;
        for (int i = 0; i < ring; i++)
        {
            Vector3 currentCP;
            if (i == 3)
                currentCP = vertices[0] + (vertices[3] - vertices[0]) / 2;
            else if (i == 4)
                currentCP = vertices[3] + (vertices[5] - vertices[3]) / 2;
            else
                currentCP = centerPoint;

            vertices.Add(vertices[i] + (currentCP - vertices[i]).normalized * map.wallDepth);
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

        Vector3 centerPoint = vertices[0] + (vertices[2] - vertices[0]) / 2 + Vector3.up * map.wallHeight; ;
        for (int i = 0; i < ring; i++)
            vertices.Add(vertices[i + ring] + (centerPoint - vertices[i+ring]).normalized*map.wallDepth);
        centerPoint = vertices[0] + (vertices[2] - vertices[0]) / 2;
        for (int i = 0; i < ring; i++)
            vertices.Add(vertices[i] + (centerPoint - vertices[i]).normalized * map.wallDepth);

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

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];//Keeps track of the already checked positions
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(map.mapCentre);
        mapFlags[map.mapCentre.x, map.mapCentre.y] = true;//Is empty (accessible)

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {//Guarantees it is inside the obstacle map
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])//We didn't check yet, and it's not an obstacle
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }        
        }

        int targetAccessibleTileCount = (int)(map.mapSize.x * map.mapSize.y - currentObstacleCount);//All the tiles that are not obstacles should be accessible
        return targetAccessibleTileCount == accessibleTileCount;
    }

    public Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-map.mapSize.x / 2f + 0.5f + x, 0, -map.mapSize.y / 2f + 0.5f + y) *tileSize;
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x,randomCoord.y];
    }

    public Transform GetEdgeTile()
    {
        return tileMap[map.mapSize.x - 2, map.mapSize.y / 2];
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
