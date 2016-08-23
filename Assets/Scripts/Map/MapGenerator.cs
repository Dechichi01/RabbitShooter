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

        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), map.seed));
    }

    private void GenerateFloor(Transform mapHolder)
    {
        Mesh mesh = new Mesh();
        vertices = new List<Vector3>();
        int[] floorTriangles;

        if (map.LRoom)
            GenerateLFloor(ref mesh, out floorTriangles);
        else
            GenerateQuadFloor(ref mesh, out floorTriangles);

        int[] wallTriangles = new int[3 * 2 * 6*7];
        int t = 0;
        t = CreateCubeMesh(wallTriangles,t, vertices[0], (int) Mathf.Ceil(vertices[1].x - vertices[0].x)/2, 5, 1);
        t = CreateCubeMesh(wallTriangles, t, vertices[0] + Vector3.right * (Mathf.Ceil(vertices[1].x - vertices[0].x) / 2 + 3), (int)Mathf.Ceil(vertices[1].x - vertices[0].x) / 2, 5, 1);
        t = CreateCubeMesh(wallTriangles,t, vertices[1], 1, 5, (int)Mathf.Ceil(vertices[2].z - vertices[1].z));
        t = CreateCubeMesh(wallTriangles, t, vertices[2], (int) Mathf.Ceil(vertices[3].x - vertices[2].x), 5, 1);
        t = CreateCubeMesh(wallTriangles, t, vertices[3], 1, 5, (int)Mathf.Ceil(vertices[4].z - vertices[3].z));
        t = CreateCubeMesh(wallTriangles, t, vertices[4], (int)Mathf.Ceil(vertices[5].x - vertices[4].x), 5, 1);
        t = CreateCubeMesh(wallTriangles, t, vertices[5], 1, 5, (int)Mathf.Ceil(vertices[0].z - vertices[5].z));
        //CreateCubeMesh(wallTriangles, vertices[1], )

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

    private int CreateCubeMesh(int[] triangles, int t, Vector3 start, int xSize, int ySize, int zSize)
    {
        bool inverted = xSize < 0 || ySize < 0 || zSize < 0;
        int v, initialVertexCount;
        v = initialVertexCount = vertices.Count;

        for (int i = 0; i < 2; i++)
        {
            vertices.Add(start + Vector3.up*ySize*i);
            vertices.Add(start + Vector3.right * xSize + Vector3.up * ySize * i);
            vertices.Add(start + Vector3.right * xSize+ Vector3.forward * zSize + Vector3.up * ySize * i);
            vertices.Add(start + Vector3.forward*zSize + Vector3.up * ySize * i);
        }

        int ring = 4;
        for (int i = 0; i < ring-1; i++, v++)
        {
            t = CreateQuad(triangles, t, v, v + 1, v + ring, v + ring + 1, inverted);
        }
        t = CreateQuad(triangles, t, v, initialVertexCount, v + ring, initialVertexCount + ring, inverted);

        t = CreateQuad(triangles, t, initialVertexCount + ring, initialVertexCount + ring + 1, initialVertexCount + ring + 3, initialVertexCount + ring + 2, inverted);
        t = CreateQuad(triangles, t, initialVertexCount, initialVertexCount + ring - 1, initialVertexCount + 1, initialVertexCount + 2, inverted);

        return t;
    }

    private void GenerateQuadFloor(ref Mesh mesh, out int[] triangles)
    {
        triangles = new int[6];

        vertices.Add(CoordToPosition(0, 0));
        vertices.Add(CoordToPosition(map.mapSize.x, 0));
        vertices.Add(CoordToPosition(0, map.mapSize.y));
        vertices.Add(CoordToPosition(map.mapSize.x, map.mapSize.y));

        CreateQuad(triangles, 0, 0, 1, 2, 3);

    }

    private void GenerateLFloor(ref Mesh mesh, out int[] triangles)
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
                Gizmos.DrawSphere(vertices[i], 0.5f);
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
