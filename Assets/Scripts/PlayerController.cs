using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	Rigidbody myRigidBody;
	Vector3 velocity;

	void Start(){
		myRigidBody = GetComponent<Rigidbody>();
	}

	public void FixedUpdate(){
		myRigidBody.MovePosition(myRigidBody.position + velocity*Time.fixedDeltaTime);
	}

	public void Move(Vector3 _velocity){
		velocity = _velocity;						
	}

	public void LookAtPoint(Vector3 lookPoint){
		Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
		transform.LookAt(heightCorrectedPoint);
	}
}
