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
		gunController = GetComponent<GunController>();
		swipeControl = GetComponent<SwipeDetector>();
        animator = GetComponent<Animator>();
		viewCamera = Camera.main;
	}

    protected override void Update()
    {
        if (recoverTime < Time.time)
        {
            isDizzy = false;
            animator.SetBool("isDizzy", false);
        }
        if (!isDizzy)
        {
            GetInputAndMove();
            HandleTouchInput();
            controller.Aim();
        }
        else
            controller.Move(Vector3.zero);
    }

    public override void TakeDamage(float damage)
    {
        if (!isDizzy)
        {
            isDizzy = true;
            animator.SetBool("isDizzy", true);
            recoverTime = Time.time + recoverDelay;
        }
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
        case SwipeDetector.SwipeDirection.Shoot:
            gunController.Shoot();
            break;
    	}
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }

}
