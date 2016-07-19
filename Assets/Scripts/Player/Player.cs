﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player : LivingEntity {

	public float moveSpeed = 5f;
	PlayerController controller;
	GunController gunController;
	Camera viewCamera;
    public Joystick rotateJoyStick;
    public RectTransform aimJoystickRect;
    private SwipeControls swipeControl;
    Vector3 aimVelocity;

	override protected void Start () {
		base.Start();
		controller = GetComponent<PlayerController>();
		gunController = GetComponent<GunController>();
		swipeControl = GetComponent<SwipeControls>();
		viewCamera = Camera.main;
	}

	void Update () {

		GetInputAndMove ();
        //GetInputAndRotate();
        GetSwipeAndAim();

		//GetInputAndLook (); 

		//if (Input.GetMouseButton(0)){
		//	gunController.Shoot();
		//}

	}

	void GetInputAndMove ()
	{
		Vector3 moveInput = new Vector3 (CrossPlatformInputManager.GetAxisRaw ("HorzLeft"), 0, CrossPlatformInputManager.GetAxisRaw ("VertLeft"));
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move (moveVelocity);
	}

    void GetInputAndRotate()
    {
    	if (CrossPlatformInputManager.GetAxisRaw("HorzRight") != 0 || CrossPlatformInputManager.GetAxisRaw("VertRight") !=0)
    	{
			Vector3 vec = (rotateJoyStick.transform.position - rotateJoyStick.m_StartPos).normalized;
        	vec = new Vector3(vec.x,0,vec.y);
        	Vector3 point = Vector3.zero + vec*10f;
        	controller.LookAtPoint(point);
    	}
    }

    void GetSwipeAndAim()
    {
    	Vector2 point;
    	bool boolean = RectTransformUtility.RectangleContainsScreenPoint(aimJoystickRect, Input.mousePosition);
    	switch(swipeControl.GetSwipeDirection()){
    	case SwipeControls.SwipeDirection.Right:
			controller.Rotate(new Vector3(0f,90f,0f));
			//transform.Rotate(new Vector3(0f,90f,0f));
			break;
    	case SwipeControls.SwipeDirection.Left:
			controller.Rotate(new Vector3(0f,-90f,0f));
			//transform.Rotate(new Vector3(0f,-90f,0f));
			break;
    	}
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
