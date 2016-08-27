using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	Rigidbody myRigidBody;
	Player player;
	Vector3 velocity;
    private FieldOfView FOV;
    private Transform[] visibleTargets;

	private float turnSpeed = 5f;
    Transform currentTarget;
    Transform lastTarget;

    private bool isTurning = false;

	void Start(){
		myRigidBody = GetComponent<Rigidbody>();
		player = GetComponent<Player>();
        FOV = GetComponent<FieldOfView>();

	}

    public void Aim()
    {
        if (!isTurning)
            FindTarget();

        if (currentTarget != null)
            Debug.DrawLine(player.transform.position, currentTarget.transform.position, Color.red);
    }

	private void FixedUpdate(){
		myRigidBody.MovePosition(myRigidBody.position + velocity*Time.fixedDeltaTime);
	}

	public void Move(Vector3 _velocity){
		velocity = _velocity;						
	}

	public void LookAtPoint(Vector3 lookPoint){
		Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
		transform.LookAt(heightCorrectedPoint);
	}

	public void Rotate(Vector3 rotation)
	{
        if (!isTurning)
        {
            StopCoroutine("PerformRotation");
            StartCoroutine(PerformRotation(rotation));
        }
	}

    void FindTarget()
    {
        visibleTargets = FOV.visibleTargets.ToArray();
        if (visibleTargets.Length > 0)
        {
            if (currentTarget == null)
                currentTarget = visibleTargets[0];
        }
        else
        {
            lastTarget = currentTarget;
            currentTarget = null;
        }

        if (currentTarget != null)
            LookAtPoint(currentTarget.position);
    }

	IEnumerator PerformRotation(Vector3 rotation)
	{
        Quaternion start = player.transform.rotation;
        Quaternion end = player.transform.rotation * Quaternion.Euler(rotation);

		float rotSign = rotation.y/ Mathf.Abs(rotation.y);

        isTurning = true;

        lastTarget = currentTarget;
        currentTarget = null;

        List<Transform> targetToIgnore = new List<Transform>();
        if(visibleTargets.Length > 0)
        {
            foreach (Transform target in visibleTargets)
            {
                Vector3 dirToTarget = (target.position - player.transform.position).normalized;
                Vector3 playerDiagonal = player.transform.forward + rotSign*player.transform.right;
                //Debug.Log(target.name + ": " + Vector3.Angle(dirToTarget, playerDiagonal));
                if (!(Vector3.Angle(dirToTarget,playerDiagonal) < 45f && target != lastTarget))
                {
                    targetToIgnore.Add(target);
                }
            }
        }

        float percent = 0f;
        while (percent<1)
		{
            percent += (turnSpeed * Time.deltaTime);
            player.transform.rotation = Quaternion.Lerp(start, end, percent);

            visibleTargets = FOV.visibleTargets.ToArray();
            if (visibleTargets.Length > 0)
            {
                foreach (Transform target in visibleTargets)
                {
                    if (!targetToIgnore.Contains(target))
                    {
                        currentTarget = target;
                        LookAtPoint(target.position);
                        break; //So we break the loop
                    }
                }
            }

			yield return null;
		}

        isTurning = false;

	}
}
