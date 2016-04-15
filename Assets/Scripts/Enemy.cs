using UnityEngine;
using System.Collections;

public class Enemy : LivingEntity {

	NavMeshAgent pathFinder;
	Transform target;

	override protected void Start () {
		base.Start();
		pathFinder = GetComponent<NavMeshAgent>();
		target = GameObject.FindGameObjectWithTag("Player").transform;

		StartCoroutine(UpdatePath());
	}

	void Update () {
	}

	IEnumerator UpdatePath(){
		float refreashRate = 0.5f;

		while(target != null){
			Vector3 targetPosition = new Vector3(target.position.x, 0, target.position.z);
			if (!dead){
				pathFinder.SetDestination(targetPosition);	
			}
			yield return new WaitForSeconds(refreashRate);
		}
	}
}
