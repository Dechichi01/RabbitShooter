using UnityEngine;
using System.Collections;

public class LivingEntity : PoolObject, IDamageable {

	public float startingHealth;
	public float health { get; protected set; }
	protected bool dead;
	public event System.Action OnDeath;

    protected virtual void Start(){
		health = startingHealth;
	}

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection, float amountToFend = 0){
        //TODO: Some stuffs with hit

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
