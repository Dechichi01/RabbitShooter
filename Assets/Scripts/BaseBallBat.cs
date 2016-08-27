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
                damageableObject.TakeHit(damage, collider.transform.position, transform.forward, 2.5f);
            }
        }
    }

    IEnumerator Attack()
    {
        isUsing = true;
        float percent = 0;
        float velocity = 1 / 0.1f;

        Vector3 startRot = transform.localRotation.eulerAngles;
        float start = startRot.x;
        float end = start + 90f;
        //Quaternion start = transform.rotation;
        //Quaternion end = transform.rotation * Quaternion.Euler(transform.parent.parent.forward*-90f);
        while(percent<1)
        {
            percent += (velocity * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(Mathf.Lerp(start, end, percent), startRot.x, startRot.z);
            yield return null;
        }
        while(percent>0)
        {
            percent -= (velocity * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(Mathf.Lerp(start, end, percent), startRot.x, startRot.z);
            yield return null;
        }

        //transform.rotation = start;
        isUsing = false;
    }
}
