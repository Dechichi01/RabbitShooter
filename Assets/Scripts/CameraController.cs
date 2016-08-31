using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

    private Camera mainCamera;

    public Transform playerT;
    public float cameraSpeed = 5f;
    public float verticalOffset = 12f;
    public float horizontalOffset = 3f;
    public bool editorMode;

    private Vector3 smoothVelocity;

    private Vector3 mapBottomEdge;
    private Vector3 mapTopEdge;
    private Vector3 mapRightEdge;
    private Vector3 mapLeftEdge;

    LivingEntity targetLivingEntity;
    private bool hasTarget;

    // Use this for initialization
    void Start()
    {
        MapGenerator mapGen = GameObject.FindGameObjectWithTag("Room").GetComponent<MapGenerator>();
        mapBottomEdge = mapGen.transform.position - Vector3.forward * mapGen.map.mapSize.y*0.485f;
        mapTopEdge = mapGen.transform.position + Vector3.forward * mapGen.map.mapSize.y * 0.455f;
        mapRightEdge = mapGen.transform.position + Vector3.right * mapGen.map.mapSize.x * 0.440f;
        mapLeftEdge = mapGen.transform.position - Vector3.right * mapGen.map.mapSize.x * 0.45f;

        if (playerT != null)
        {
            hasTarget = true;
            targetLivingEntity = playerT.GetComponent<LivingEntity>();
            targetLivingEntity.OnDeath += OnPlayerDeath; //That's how we subscribe a method to a System.Action method (OnDeath)
        }

        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        SinchronizePosition();
    }

    public void SinchronizePosition()
    {
        if (hasTarget || editorMode)
        {
            transform.position = playerT.position + Vector3.up * verticalOffset + Vector3.back * horizontalOffset;
            if (!editorMode && transform.position.z < mapBottomEdge.z)
                transform.position = new Vector3(transform.position.x, transform.position.y, mapBottomEdge.z);
            else if (!editorMode && transform.position.z > mapTopEdge.z)
                transform.position = new Vector3(transform.position.x, transform.position.y, mapTopEdge.z);
            if (!editorMode && transform.position.x > mapRightEdge.x)
                transform.position = new Vector3(mapRightEdge.x, transform.position.y, transform.position.z);
            else if (!editorMode && transform.position.x < mapLeftEdge.x)
                transform.position = new Vector3(mapLeftEdge.x, transform.position.y, transform.position.z);
        }
    }

    void OnPlayerDeath()
    {
        hasTarget = false;
        editorMode = false;
        targetLivingEntity.OnDeath -= OnPlayerDeath;
    }


}
