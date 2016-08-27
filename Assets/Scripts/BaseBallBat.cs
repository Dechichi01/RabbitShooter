using UnityEngine;
using System.Collections;
using System;

public class BaseBallBat : Weapon {

    bool isUsing;

    public override void Use()
    {
        if (!isUsing)
            StartCoroutine(Attack());
    }

    public override void Equip(Transform holder)
    {
        transform.position = holder.position;
        transform.parent = holder;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (isUsing && collider.CompareTag("Enemy"))
        {
            IDamageable damageableObject = collider.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                damageableObject.TakeHit(damage, collider.transform.position, transform.forward, 2f);
            }
        }
    }

    IEnumerator Attack()
    {
        isUsing = true;
        float percent = 0;
        float velocity = 1 / 0.1f;

        Quaternion start = transform.rotation;
        Quaternion end = transform.rotation * Quaternion.Euler(Vector3.forward*-90f);
        while(percent<1)
        {
            percent += (velocity * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(start, end, percent);
            yield return null;
        }
        while(percent>0)
        {
            percent -= (velocity * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(start, end, percent);
            yield return null;
        }

        transform.rotation = start;
        isUsing = false;
    }
}
