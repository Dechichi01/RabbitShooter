using UnityEngine;
using System.Collections;

public class Enemy : LivingEntity {

	public enum State {Idle, Chasing, Attacking};
	State currentState;

	NavMeshAgent pathFinder;
	Transform target;
	LivingEntity targetLivingEntity;
	Material skinMaterial;

	Color originalColour;

	float damage = 1f;

	float attackDistanceThreshold = .5f;
	float timeBetweenAttacks = 1;

	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;

	bool hasTarget;

	override protected void Start () {
		base.Start();
		pathFinder = GetComponent<NavMeshAgent>();
		skinMaterial = GetComponent<Renderer>().material;
		originalColour = skinMaterial.color;

		if (GameObject.FindGameObjectWithTag("Player") != null){
			hasTarget = true;
			currentState = State.Chasing;
			target = GameObject.FindGameObjectWithTag("Player").transform;
			targetLivingEntity = target.GetComponent<LivingEntity>();
			targetLivingEntity.OnDeath += OnTargetDeath; //That's how we subscribe a method to a System.Action method (OnDeath)

			myCollisionRadius = GetComponent<CapsuleCollider>().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

			StartCoroutine(UpdatePath());
		}
	}

	void Update () {

		if (hasTarget){
			if (Time.time > nextAttackTime){
				float sqrDstToTarget = (target.position - transform.position).sqrMagnitude; //take the distance between two positions in sqrMagnitude

				if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)){
					nextAttackTime = Time.time + timeBetweenAttacks;	
					StartCoroutine(Attack());	
				}	
			}
		}
	}

	void OnTargetDeath(){
		hasTarget = false;
        targetLivingEntity.OnDeath -= OnTargetDeath;//Good practice to unsubscribe from a method
		currentState = State.Idle;
	}

	IEnumerator Attack(){

		currentState = State.Attacking;
		pathFinder.enabled = false;

		Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - dirToTarget*(myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;
		bool hasAppliedDamage = false;

		skinMaterial.color = Color.red;

		while (percent <= 1){

			if (percent >= 0.5 && !hasAppliedDamage){
				hasAppliedDamage = true;
				targetLivingEntity.TakeDamage(damage);
			}

			percent += Time.deltaTime*attackSpeed;
			float interpolation = (-Mathf.Pow(percent,2) + percent)*4;
			transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

			yield return null;
		}

		skinMaterial.color = originalColour;
		currentState = State.Chasing;
		pathFinder.enabled = true;
	}

	IEnumerator UpdatePath(){
		float refreashRate = 0.5f;

		while(hasTarget){
			if (currentState == State.Chasing){
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - dirToTarget*(myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
				if (!dead){
					pathFinder.SetDestination(targetPosition);	
				}
			}
			yield return new WaitForSeconds(refreashRate);
		}
	}
}
