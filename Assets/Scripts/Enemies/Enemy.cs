using UnityEngine;
using System.Collections;

public class Enemy : LivingEntity {

	public enum State {Idle, Chasing, Attacking};
	State currentState;

	NavMeshAgent navAgent;
	Transform playerTarget;
    Vector3 babyCribPos;
	LivingEntity targetLivingEntity;
	Material skinMaterial;

	Color originalColour;

    public ParticleSystem deathEffect;

    public static event System.Action OnDeathStatic;//Used by the ScoreKeeper

	float damage = 1f;

	float attackDistanceThreshold = .5f;
	float timeBetweenAttacks = 1;

	float nextAttackTime;
	float myCollisionRadius;
	float secondaryTargetCollisionRadius;
    float primaryTargetCollisionRadius;

    bool chasingPrimaryTarget;
    bool hasTarget;
    public bool stationary;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.enabled = false;

        myCollisionRadius = GetComponent<CapsuleCollider>().radius;

        if (GameObject.FindGameObjectWithTag("BabyCrib") != null)
        {
            chasingPrimaryTarget = true;
            hasTarget = true;

            babyCribPos = GameObject.FindGameObjectWithTag("BabyCrib").transform.position;
            Debug.Log(babyCribPos);
            primaryTargetCollisionRadius = 3.5f;
        }

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
            targetLivingEntity = playerTarget.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            secondaryTargetCollisionRadius = playerTarget.GetComponent<CapsuleCollider>().radius;
        }
    }

	override protected void Start () {
		base.Start();
        //StartChase();
	}

    public void StartChase()
    {
        navAgent.enabled = true;
        if (hasTarget)
        {
            currentState = State.Chasing;
            targetLivingEntity.OnDeath += OnTargetDeath; //That's how we subscribe a method to a System.Action method (OnDeath)

            if (!stationary)
                StartCoroutine(UpdatePath());
        }
    }

	void Update () {
		if (hasTarget){
			if (Time.time > nextAttackTime){
                Vector3 targetPos = (chasingPrimaryTarget) ? babyCribPos : playerTarget.position;
				float sqrDstToTarget = (targetPos - transform.position).sqrMagnitude; //take the distance between two positions in sqrMagnitude

                float radius = (chasingPrimaryTarget) ? primaryTargetCollisionRadius : secondaryTargetCollisionRadius;
				if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + radius, 2)){
					nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
					StartCoroutine(Attack());	
				}	
			}
		}
	}

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health)
        {
            if (OnDeathStatic != null)
                OnDeathStatic();
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward,hitDirection)) as GameObject, deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath(){
		hasTarget = false;
        targetLivingEntity.OnDeath -= OnTargetDeath;//Good practice to unsubscribe from a method
		currentState = State.Idle;
	}

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
    {
        navAgent.speed = moveSpeed;
        if (hasTarget)
            damage = Mathf.Ceil(targetLivingEntity.startingHealth / hitsToKillPlayer);

        startingHealth = enemyHealth;
        //Material universalSkin = GetComponent<Renderer>().sharedMaterial;
        //universalSkin.color = skinColor;
        //originalColour = universalSkin.color;
        //skinMaterial = GetComponent<Renderer>().material;

    }
	IEnumerator Attack(){

		currentState = State.Attacking;
		navAgent.enabled = false;

        Vector3 targetPos = (chasingPrimaryTarget) ? babyCribPos : playerTarget.position;
        Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (targetPos - transform.position).normalized;
		Vector3 attackPosition = targetPos - dirToTarget*(myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;
		bool hasAppliedDamage = false;

		//skinMaterial.color = Color.red;

		while (percent <= 1){

			if (!chasingPrimaryTarget && percent >= 0.5 && !hasAppliedDamage){
				hasAppliedDamage = true;
				targetLivingEntity.TakeDamage(damage);
			}

			percent += Time.deltaTime*attackSpeed;
			float interpolation = (-Mathf.Pow(percent,2) + percent)*4;
			transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

			yield return null;
		}

		//skinMaterial.color = originalColour;
		currentState = State.Chasing;
		navAgent.enabled = true;
	}

	IEnumerator UpdatePath(){            
        float refreashRate = 0.5f;

        Vector3 currentTargetPos = (chasingPrimaryTarget) ? babyCribPos : playerTarget.position;
        float radius = (chasingPrimaryTarget) ? primaryTargetCollisionRadius : secondaryTargetCollisionRadius;
        while (hasTarget){
			if (currentState == State.Chasing){
				Vector3 dirToTarget = (currentTargetPos - transform.position).normalized;
				Vector3 targetPosition = currentTargetPos - dirToTarget*(myCollisionRadius + radius + attackDistanceThreshold/2);
				if (!dead){
					navAgent.SetDestination(targetPosition);	
				}
			}
			yield return new WaitForSeconds(refreashRate);
		}
	}
}
