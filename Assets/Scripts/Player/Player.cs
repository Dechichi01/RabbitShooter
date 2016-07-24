using UnityEngine;
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
    private SwipeDetector swipeControl;
    Vector3 aimVelocity;

    private Transform currentVisibleTarget;
    private Transform lastVisibleTarget;

	override protected void Start () {
		base.Start();
		controller = GetComponent<PlayerController>();
		gunController = GetComponent<GunController>();
		swipeControl = GetComponent<SwipeDetector>();
		viewCamera = Camera.main;
	}

	void Update () {

		GetInputAndMove ();
        HandleTouchInput();           

	}

	void GetInputAndMove ()
	{
		Vector3 moveInput = new Vector3 (CrossPlatformInputManager.GetAxisRaw ("HorzLeft"), 0, CrossPlatformInputManager.GetAxisRaw ("VertLeft"));
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move (moveVelocity);
	}

    void HandleTouchInput()
    {
    	switch(swipeControl.GetSwipeDirection())
        {
    	case SwipeDetector.SwipeDirection.Right:
			controller.Rotate(new Vector3(0f,90f,0f));
			break;
        case SwipeDetector.SwipeDirection.Left:
			controller.Rotate(new Vector3(0f,-90f,0f));
			break;
        case SwipeDetector.SwipeDirection.Shoot:
            gunController.Shoot();
            break;
    	}
    }

}
