using UnityEngine;
using System.Collections;
using Panda;

public class Enemy : LivingEntity {

	public enum State {Idle, Chasing, Attacking};
	State currentState;

    /// <summary>
    /// Used by the AI BT
    /// </summary>
    [Task]
    public bool shouldChaseCrib { get { return (!(currentState == State.Attacking) && !isBeingAttacked) /*playerIsDizzy*/; } }
    [Task]
    public bool shouldChasePlayer { get { return !shouldChaseCrib ; } }
    [Task]
    public bool canAttack { get
        {
            if (hasTarget)
            {
                if (Time.time > nextAttackTime)
                {
                    float sqrDstToTarget = (currentTarget.thisTransform.position - transform.position).sqrMagnitude; //take the distance between two positions in sqrMagnitude
                    float horizontalDist = Mathf.Pow(attackDistanceThreshold + myCollisionRadius + currentTarget.minPlaneDist.x, 2);
                    float verticalDist = Mathf.Pow(attackDistanceThreshold + myCollisionRadius + currentTarget.minPlaneDist.y, 2);
                    if (sqrDstToTarget < horizontalDist || sqrDstToTarget < verticalDist)
                    {
                        nextAttackTime = Time.time + timeBetweenAttacks;
                        return true;
                    }
                }
            }
            return false;
        }
    }

	NavMeshAgent navAgent;

	Target currentTarget;
    Target playerTarget;
    Target babyCrib;

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

    bool hasTarget;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.enabled = false;

        myCollisionRadius = GetComponent<CapsuleCollider>().radius;

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            Transform playerT = GameObject.FindGameObjectWithTag("Player").transform;
            playerTarget = new Target(playerT, Vector2.one*playerT.GetComponent<CapsuleCollider>().radius);
            targetLivingEntity = playerTarget.thisTransform.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        }

        if (FindObjectOfType<BabyCrib>() !=null)
        {
            babyCrib = new Target(FindObjectOfType<BabyCrib>().transform, new Vector2(2,3));
            Debug.Log(FindObjectOfType<BabyCrib>().transform.name);
        }

        currentTarget = babyCrib;
    }

	override protected void Start () {
		base.Start();
	}

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TakeHit(10, Vector3.zero, Vector3.zero);
        base.Update();
    }

    [Task]
    public void StartChase()
    {
        navAgent.enabled = true;
        if (hasTarget)
        {
            currentState = State.Chasing;
            targetLivingEntity.OnDeath += OnTargetDeath; //That's how we subscribe a method to a System.Action method (OnDeath)

            StartCoroutine(UpdatePath());
            Task.current.Succeed();
        }
        Task.current.Succeed();
    }

    [Task]
    void Attack(int target)
    {
        AudioManager.instance.PlaySound("Enemy Attack", transform.position);
        StartCoroutine(AttackPlayer());
        Task.current.Succeed();
    }

    [Task]
    void SetTarget(int target)
    {
        if (target == 1)
            currentTarget = babyCrib;
        else
            currentTarget = playerTarget;

        Task.current.Succeed();
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

    }
	IEnumerator AttackPlayer(){

		currentState = State.Attacking;
		navAgent.enabled = false;

        Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (currentTarget.thisTransform.position - transform.position).normalized;
		Vector3 attackPosition = currentTarget.thisTransform.position - dirToTarget*(myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;
		bool hasAppliedDamage = false;


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

		//skinMaterial.color = originalColour;
		currentState = State.Chasing;
		navAgent.enabled = true;
	}

	IEnumerator UpdatePath(){            
        float refreashRate = 0.5f;

        while (hasTarget){
			if (currentState == State.Chasing){
				Vector3 dirToTarget = (currentTarget.thisTransform.position - transform.position).normalized;
				Vector3 targetPosition = currentTarget.thisTransform.position - dirToTarget*(myCollisionRadius + currentTarget.minPlaneDist.x + attackDistanceThreshold/2);
				if (!dead){
					navAgent.SetDestination(targetPosition);	
				}
			}
			yield return new WaitForSeconds(refreashRate);
		}
	}
}

public class Target
{
    public Transform thisTransform;
    public Vector2 minPlaneDist;

    public Target(Transform thisTransform, Vector2 minPlaneDist)
    {
        this.thisTransform = thisTransform;
        this.minPlaneDist = minPlaneDist;
    }
}

