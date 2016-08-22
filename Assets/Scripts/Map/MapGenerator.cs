using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : Module
{
    public Map map;

    public Transform tilePrefab;
    public Transform wallPrefab;
    public RoomDoor doorPrefab;
    [Range (1,4)]
    public int doorQuantity;
    public Obstacle[] furnitures;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    public Transform[] obstaclePrefabs;

    public Vector2 maxMapSize;

    List<Coord> allTileCoords;
    List<Coord> allOpenCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform[,] tileMap;

    [HideInInspector]
    public float tileSize = 2.4f;

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
        tileMap = new Transform[map.mapSize.x, map.mapSize.y];
        GetComponent<BoxCollider>().size = new Vector3(map.mapSize.x * tileSize, .05f, map.mapSize.y * tileSize);

        allTileCoords = new List<Coord>();//Store all the x,y coordinates in our map
        //Store all coordinates
        for (int x = 0; x < map.mapSize.x; x++)
            for (int y = 0; y < map.mapSize.y; y++)
                allTileCoords.Add(new Coord(x, y));

        allOpenCoords = new List<Coord>(allTileCoords);//Copy of allTileCoords to be store only the open walls in the end

        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
            DestroyImmediate(transform.FindChild(holderName).gameObject);

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        InstantiateTiles(mapHolder);
        InstantiateWalls(mapHolder);
        InstantiateFurniture(mapHolder);
        //Store all shuffled coordinates
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), map.seed));

        InstantiateObstacles(mapHolder);
        InstantiateNavMask(mapHolder);

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

    private void InstantiateTiles(Transform mapHolder)
    {
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }
    }

    private void InstantiateWalls(Transform mapHolder)
    {
        float[] doorPositions = new float[doorQuantity];
        for (int i = 0; i < (doorQuantity+1)/2; i++)
        {
            doorPositions[i] = Random.Range(2, map.mapSize.x - 2);
        }
        for (int i = (doorQuantity + 1) / 2; i < doorQuantity; i++)
        {
            doorPositions[i] = Random.Range(2, map.mapSize.y - 2);
        }

        int index = 0;
        for (int x = 0; x < map.mapSize.x; x++)
        {
            if (index < doorQuantity && doorPositions[index] == x)
            {
                RoomDoor door = Instantiate(doorPrefab) as RoomDoor;
                float doorHeight = door.doorBC.bounds.extents.y;
                door.transform.position = CoordToPosition(x, map.mapSize.y - 1) + Vector3.up * doorHeight + Vector3.forward * tileSize / 2;
                door.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                door.transform.parent = mapHolder;
                door.connector.Room = transform;
                allOpenCoords.Remove(new Coord(x, map.mapSize.y -1));
                allOpenCoords.Remove(new Coord(x+1, map.mapSize.y - 1));
                x++;
            }
            else
            {
                Transform newWall = Instantiate(wallPrefab) as Transform;
                float wallHeight = newWall.GetComponent<BoxCollider>().bounds.extents.y;
                newWall.position = CoordToPosition(x, map.mapSize.y - 1) + Vector3.up * wallHeight + Vector3.forward * tileSize / 2;
                newWall.rotation = Quaternion.identity;
                newWall.localScale = new Vector3(tileSize, newWall.localScale.y, newWall.localScale.z);
                newWall.parent = mapHolder;
                allOpenCoords.Remove(new Coord(x, map.mapSize.y - 1));
            }
        }
        index++;

        for (int x = 0; x < map.mapSize.x; x++)
        {
            if (index < doorQuantity && doorPositions[index] == x)
            {
                RoomDoor door = Instantiate(doorPrefab) as RoomDoor;
                float doorHeight = door.doorBC.bounds.extents.y;
                door.transform.position = CoordToPosition(x, 0) + Vector3.up * doorHeight - Vector3.forward * tileSize / 2 + Vector3.right*tileSize;
                door.transform.localRotation = Quaternion.identity;
                door.transform.parent = mapHolder;
                door.connector.Room = transform;
                allOpenCoords.Remove(new Coord(x, 0));
                allOpenCoords.Remove(new Coord(x+1, 0));
                x++;
            }
            else
            {
                Transform newWall = Instantiate(wallPrefab) as Transform;
                float wallHeight = newWall.GetComponent<BoxCollider>().bounds.extents.y;
                newWall.position = CoordToPosition(x, 0) + Vector3.up * wallHeight - Vector3.forward * tileSize / 2;
                newWall.rotation = Quaternion.identity;
                newWall.localScale = new Vector3(tileSize, newWall.localScale.y, newWall.localScale.z);
                newWall.parent = mapHolder;
                allOpenCoords.Remove(new Coord(x, 0));
            }
        }
        index++;
        for (int y = 0; y < map.mapSize.y; y++)
        {
            if (index < doorQuantity && doorPositions[index] == y)
            {
                RoomDoor door = Instantiate(doorPrefab) as RoomDoor;
                float doorHeight = door.doorBC.bounds.extents.y;
                door.transform.position = CoordToPosition(0, y) + Vector3.up * doorHeight - Vector3.right * tileSize / 2;
                door.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                door.transform.parent = mapHolder;
                door.connector.Room = transform;
                allOpenCoords.Remove(new Coord(0, y));
                allOpenCoords.Remove(new Coord(0, y+1));
                y++;
            }
            else
            {
                Transform newWall = Instantiate(wallPrefab) as Transform;
                float wallHeight = newWall.GetComponent<BoxCollider>().bounds.extents.y;
                newWall.position = CoordToPosition(0, y) + Vector3.up * wallHeight - Vector3.right * tileSize / 2;
                newWall.rotation = Quaternion.Euler(0f, 90f, 0f);
                newWall.localScale = new Vector3(tileSize, newWall.localScale.y, newWall.localScale.z);
                newWall.parent = mapHolder;
                allOpenCoords.Remove(new Coord(0, y));
            }
        }
        index++;
        for (int y = 0; y < map.mapSize.y; y++)
        {
            if (index < doorQuantity && doorPositions[index] == y)
            {
                RoomDoor door = Instantiate(doorPrefab) as RoomDoor;
                float doorHeight = door.doorBC.bounds.extents.y;
                door.transform.position = CoordToPosition(map.mapSize.x - 1, y) + Vector3.up * doorHeight + Vector3.right * tileSize / 2 + Vector3.forward*tileSize;
                door.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                door.transform.parent = mapHolder;
                door.connector.Room = transform;
                allOpenCoords.Remove(new Coord(map.mapSize.x - 1, y));
                allOpenCoords.Remove(new Coord(map.mapSize.x - 1, y+1));
                y++;
            }
            else
            {
                Transform newWall = Instantiate(wallPrefab) as Transform;
                float wallHeight = newWall.GetComponent<BoxCollider>().bounds.extents.y;
                newWall.position = CoordToPosition(map.mapSize.x - 1, y) + Vector3.up * wallHeight + Vector3.right * tileSize / 2;
                newWall.rotation = Quaternion.Euler(0f, -90f, 0f);
                newWall.localScale = new Vector3(tileSize, newWall.localScale.y, newWall.localScale.z);
                newWall.parent = mapHolder;
                allOpenCoords.Remove(new Coord(map.mapSize.x - 1, y));
            }
        }
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
    public Coord mapSize;
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
