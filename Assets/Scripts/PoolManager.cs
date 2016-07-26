using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {

    private Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>>();

    static PoolManager _instance;
    public static PoolManager instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<PoolManager>();
            return _instance;
        }
    }

    /*void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);
        else
            _instance = this;
    }*/
	public void CreatePool(GameObject prefab, int poolSize)
    {
        int poolKey = prefab.GetInstanceID();

        GameObject poolHolder = new GameObject(prefab.name + " Pool");
        poolHolder.transform.parent = transform;
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<ObjectInstance>());
            for (int i = 0; i < poolSize; i++)
            {
                ObjectInstance newObject = new ObjectInstance(Instantiate(prefab) as GameObject);
                poolDictionary[poolKey].Enqueue(newObject);
                newObject.SetParent(poolHolder.transform);
            }
        }
    }

    public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            ObjectInstance objectToReuse = poolDictionary[poolKey].Dequeue();
            poolDictionary[poolKey].Enqueue(objectToReuse);

            objectToReuse.Reuse(position, rotation);
            return objectToReuse.gameObject;
        }
        return null;
    }

    public class ObjectInstance
    {
        public GameObject gameObject { get; private set; }
        Transform transform;

        bool hasPoolObjectComponent;
        PoolObject poolObjectScript;

        public ObjectInstance(GameObject objectInstance)
        {
            gameObject = objectInstance;
            transform = gameObject.transform;
            gameObject.SetActive(false);
            if (gameObject.GetComponent<PoolObject>())
            {
                hasPoolObjectComponent = true;
                poolObjectScript = gameObject.GetComponent<PoolObject>();
            }
        }

        public void Reuse(Vector3 position, Quaternion rotation)
        {
            if (hasPoolObjectComponent)
            {
                transform.position = position;
                transform.rotation = rotation;
                gameObject.SetActive(true);

                poolObjectScript.OnObjectReuse();
            }
        }

        public void SetParent(Transform parent)
        {
            transform.parent = parent;
        }
    }
}
