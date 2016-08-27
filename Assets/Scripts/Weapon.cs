using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour {

    protected float damage = 1f;

    public abstract void Use();

    public abstract void Equip(Transform holder);
}
