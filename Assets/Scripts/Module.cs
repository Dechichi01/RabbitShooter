using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Module : MonoBehaviour {

    public string Tag;

    private MeshCollider meshCol;

    public Bounds bounds;

    void Start()
    {
        meshCol = transform.GetChild(1).FindChild("Floor").GetComponent<MeshCollider>();
        bounds = meshCol.bounds;
    }

    public List<Exit> GetExits()
    {
        return new List<Exit>(GetComponentsInChildren<Exit>());
    }
}
