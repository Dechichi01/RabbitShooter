using UnityEngine;
using System.Collections;

public class LivingEntity : PoolObject, IDamageable {

	public float startingHealth;
	public float health { get; protected set; }
	protected bool dead;
	public event System.Action OnDeath;

    protected bool isBeingAttacked;
    private float beingAttackedDelay = 10;
    private float timeToResetBeingAttacked;

    public bool isDizzy;//used by the player

    protected virtual void Start(){
		health = startingHealth;
	}

    protected virtual void Update()
    {
        if (Time.time > timeToResetBeingAttacked)
            isBeingAttacked = false;
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection){
        //TODO: Some stuffs with hit
        isBeingAttacked = true;
        timeToResetBeingAttacked = Time.time + beingAttackedDelay;

        TakeDamage(damage);
	}

	public virtual void TakeDamage(float damage){
		health -= damage;

        if (health <= 0 && !dead){
			Die();
		}		
	}

    [ContextMenu("Self Destruct")]
	public virtual void Die(){
		dead = true;
		if (OnDeath != null){
			OnDeath();
            OnDeath = null;//Make all methods unsubscribe from OnDeath
		}
        if (GetComponent<Enemy>())
            Destroy();
        else
		    GameObject.Destroy(gameObject);
	}
}
