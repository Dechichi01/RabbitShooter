using UnityEngine;
using System.Collections;

public class LivingEntity : MonoBehaviour, IDamageable {

	public float startingHealth;
	protected float health;
	protected bool dead;

    public ParticleSystem bloodEffect;

	public event System.Action OnDeath;

	protected virtual void Start(){
		health = startingHealth;
	}

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection){
        //TODO: Some stuffs with hit
        System.Random rand = new System.Random((int)Time.time);

        if (bloodEffect != null)
            Destroy(Instantiate(bloodEffect.gameObject, hitPoint, Quaternion.Euler(rand.Next(-50, 50),rand.Next(-180, 180), rand.Next(-50, 50))) as GameObject, bloodEffect.startLifetime);
        TakeDamage(damage);
	}

	public virtual void TakeDamage(float damage){
		health -= damage;

        if (health <= 0 && !dead){
			Die();
		}		
	}

    [ContextMenu("Self Destruct")]
	protected void Die(){
		dead = true;
		if (OnDeath != null){
			OnDeath();
		}
		GameObject.Destroy(gameObject);
	}
}
