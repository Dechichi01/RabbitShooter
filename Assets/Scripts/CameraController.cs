using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    private Camera mainCamera;

    public Transform playerT;
    public float cameraSpeed = 5f;
    public float verticalOffset = 12f;
    public float horizontalOffset = 3f;
    public bool editorMode;

    private Vector3 smoothVelocity;

    LivingEntity targetLivingEntity;
    private bool hasTarget;

	// Use this for initialization
	void Start () {
        //playerT = GameObject.FindWithTag("Player").GetComponent<Transform>();

        if (playerT != null)
        {
            hasTarget = true;
            targetLivingEntity = playerT.GetComponent<LivingEntity>();
            targetLivingEntity.OnDeath += OnPlayerDeath; //That's how we subscribe a method to a System.Action method (OnDeath)
        }

        mainCamera = Camera.main;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        SinchronizePosition();
    }
    
    public void SinchronizePosition()
    {
        if (hasTarget || editorMode)
        {
            transform.position = playerT.position + Vector3.up * verticalOffset + Vector3.back * horizontalOffset;
        }
    }

    void OnPlayerDeath()
    {
        hasTarget = false;
        editorMode = false;
        targetLivingEntity.OnDeath -= OnPlayerDeath;
    }


}
