using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    private Transform playerT;
    private Camera mainCamera;

    private float xMax, yMax;
    private float xMin, yMin;

    public bool disableXFollow;
    public bool disableYFollow;

    public float cameraSpeed = 5f;

    private Vector3 smoothVelocity;

    LivingEntity targetLivingEntity;
    private bool hasTarget;

    bool isTranslating;
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
        xMax = mainCamera.pixelWidth*0.8f;
        xMin = mainCamera.pixelWidth * 0.2f;
        yMax = mainCamera.pixelHeight*0.9f;
        yMin = mainCamera.pixelHeight * 0.1f;
	}
	
	// Update is called once per frame
	void LateUpdate () {

        if (hasTarget)
        {
            Vector3 playerScreenPos = mainCamera.WorldToScreenPoint(playerT.position);

            Vector3 translation = Vector3.zero;
            if (!disableXFollow)
            {
                if (playerScreenPos.x > xMax && !isTranslating)
                    translation += (Vector3.right * 10);
                if (playerScreenPos.x < xMin && !isTranslating)
                    translation += (Vector3.left * 10);
            }

            if (!disableYFollow)
            {
                if (playerScreenPos.y > yMax && !isTranslating)
                    translation += (Vector3.forward * 6);
                if (playerScreenPos.y < yMin && !isTranslating)
                    translation += (Vector3.back * 6);
            }
                
            if (translation != Vector3.zero)
                StartCoroutine(TranslateCamera(translation));
        }
    }

    void OnPlayerDeath()
    {
        hasTarget = false;
        targetLivingEntity.OnDeath -= OnPlayerDeath;
    }

    IEnumerator TranslateCamera(Vector3 translation)
    {
        isTranslating = true;
        Vector3 targetPosition = mainCamera.transform.position + translation;
        while(true)
        {
            mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPosition, ref smoothVelocity, 0.5f);
            //Debug.Log("x: " + Mathf.RoundToInt(mainCamera.transform.position.x - targetPosition.x) + ", z: " + Mathf.RoundToInt(mainCamera.transform.position.z - targetPosition.z));
            float xDiff = Mathf.RoundToInt(mainCamera.transform.position.x - targetPosition.x);
            float zDiff = Mathf.RoundToInt(mainCamera.transform.position.z - targetPosition.z);
            if (xDiff == 0 && zDiff == 0)
            {
                isTranslating = false;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }


}
