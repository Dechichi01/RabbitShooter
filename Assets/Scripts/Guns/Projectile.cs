using UnityEngine;
using System.Collections;

public class Projectile : PoolObject {

	public LayerMask collisionMask;
	float speed = 100f;
	float damage = 1f;

	float lifeTime = 3f;
	float skinWidth = .1f;

	void Start(){
        Destroy(lifeTime);

		Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);
		if (initialCollisions.Length > 0){
			OnHitObject(initialCollisions[0], transform.position);		
		}
	}

	public void SetSpeed(float newSpeed){
		speed = newSpeed;
	}

	void Update () {
		float moveDistance = speed*Time.deltaTime;
		CheckCollisions(moveDistance);
		transform.Translate(Vector3.forward*moveDistance);

	}

	void CheckCollisions(float moveDistance){
		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)){
			OnHitObject(hit.collider, hit.point);
		}
	}

	void OnHitObject(Collider c, Vector3 hitPoint){
		IDamageable damageableObject = c.GetComponent<IDamageable>();
		if (damageableObject != null){
			damageableObject.TakeHit(damage, hitPoint, transform.forward);
		}
		Destroy();
	}
}
