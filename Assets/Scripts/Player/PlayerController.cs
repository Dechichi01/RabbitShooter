using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	Rigidbody myRigidBody;
	Player player;
	Vector3 velocity;

	private float turnSpeed = 10f;

	void Start(){
		myRigidBody = GetComponent<Rigidbody>();
		player = GetComponent<Player>();
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
		StopCoroutine("PerformRotation");
		StartCoroutine(PerformRotation(rotation));
	}

	IEnumerator PerformRotation(Vector3 rotation)
	{
		float remainingRot = Mathf.Abs(rotation.y);
		float rotSign = rotation.y/remainingRot;
		float rotEachFrame = rotation.y;
		while(true)
		{
			remainingRot-=(rotEachFrame*Time.deltaTime*turnSpeed*rotSign);
			player.transform.Rotate(Vector3.up*rotEachFrame*Time.deltaTime*turnSpeed);
			if (remainingRot <=0)
				break;
			yield return new WaitForFixedUpdate();
		}
		yield return null;
	}
}
