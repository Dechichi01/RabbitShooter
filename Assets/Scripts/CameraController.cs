using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    private Transform playerT;
    private Camera mainCamera;

    public float cameraSpeed = 5f;

    private Vector3 smoothVelocity;

    LivingEntity targetLivingEntity;
    private bool hasTarget;

	// Use this for initialization
	void Start () {
        playerT = GameObject.FindWithTag("Player").GetComponent<Transform>();
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

        if (hasTarget)
        {
            mainCamera.transform.position = playerT.position + Vector3.up * 12 + Vector3.back * 3;
        }
    }

    void OnPlayerDeath()
    {
        hasTarget = false;
        targetLivingEntity.OnDeath -= OnPlayerDeath;
    }


}
