using UnityEngine;
using System.Collections;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player : LivingEntity {

	public float moveSpeed = 5f;
	PlayerController controller;
	GunController gunController;

	Camera viewCamera;

	override protected void Start () {
		base.Start();
		controller = GetComponent<PlayerController>();
		gunController = GetComponent<GunController>();
		viewCamera = Camera.main;
	}

	void Update () {
		GetInputAndMove ();

		GetInputAndLook (); 

		if (Input.GetMouseButton(0)){
			gunController.Shoot();
		}

	}

	void GetInputAndMove ()
	{
		Vector3 moveInput = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move (moveVelocity);
	}

	void GetInputAndLook ()
	{
		Ray ray = viewCamera.ScreenPointToRay (Input.mousePosition);
		//return a ray that start in the camera and goes into the given position
		Plane groundPlane = new Plane (Vector3.up, Vector3.zero);
		float rayDistance;
		//Check the intercection and return the value in rayDistance
		if (groundPlane.Raycast (ray, out rayDistance)) {
			Vector3 point = ray.GetPoint (rayDistance);
			//Debug.DrawLine(ray.origin, point, Color.red);
			controller.LookAtPoint (point);
		}
	}
}
