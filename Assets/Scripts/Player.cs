using UnityEngine;
using System.Collections;

[RequireComponent (typeof (PlayerController))]
public class Player : MonoBehaviour {

	public float moveSpeed = 5f;
	
	Camera viewCamera;
	PlayerController controller;

	// Use this for initialization
	void Start () {
		controller = GetComponent<PlayerController>();
		viewCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move(moveVelocity);
		
		Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector2.up, Vector2.zero);
		float rayDistance;
		
		if (groundPlane.Raycast(ray,out rayDistance)){ //will return true if the ray intercepts with the ground (will return the length)
			Vector3 interceptionPoint = ray.GetPoint(rayDistance);
			//Debug.DrawLine(ray.origin, interceptionPoint, Color.red);
			controller.LookAt(interceptionPoint);
		}
	}
}
