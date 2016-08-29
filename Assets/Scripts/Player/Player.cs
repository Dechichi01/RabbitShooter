using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (WeaponManager))]
public class Player : LivingEntity {

	public float moveSpeed = 5f;
    public Weapon[] weapons;
    private int currentHoldWeaponIndex;

	PlayerController controller;
	WeaponManager weaponManager;
	Camera viewCamera;
    private SwipeDetector swipeControl;
    Vector3 aimVelocity;

    private Transform currentVisibleTarget;
    private Transform lastVisibleTarget;

    private Animator animator;
    private float recoverDelay = 3;
    private float recoverTime;

	override protected void Start () {
		base.Start();
		controller = GetComponent<PlayerController>();
		weaponManager = GetComponent<WeaponManager>();
		swipeControl = GetComponent<SwipeDetector>();
        animator = GetComponent<Animator>();
		viewCamera = Camera.main;
	}

    void Update()
    {
        GetInputAndMove();
        HandleTouchInput();
        controller.Aim();      
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
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
            case SwipeDetector.SwipeDirection.Jump:
                controller.Rotate(new Vector3(0f, 90f, 0f));
                break;
            case SwipeDetector.SwipeDirection.Duck:
                controller.Rotate(new Vector3(0f, -90f, 0f));
                break;
            case SwipeDetector.SwipeDirection.ChangeWeapon:
                weaponManager.EquipGun(weapons[(++currentHoldWeaponIndex) % weapons.Length]);
                break;
            case SwipeDetector.SwipeDirection.Attack:
                weaponManager.Use();
                break;
    	}

    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("DoorEntrance") || col.CompareTag("DoorExit"))
        {
            col.transform.parent.GetComponent<RoomDoor>().OpenDoor();
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("DoorEntrance") || col.CompareTag("DoorExit"))
        {
            col.transform.parent.GetComponent<RoomDoor>().CloseDoor();
        }
    }

}
