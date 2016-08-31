using UnityEngine;
using System.Collections;

public class BreakableObstacle : Obstacle {

    public ParticleSystem deathEffect;

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection, float amountToFend = 0)
    {
        if (damage>= health)
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
        base.TakeHit(damage, hitPoint, hitDirection, amountToFend);
    }
}
