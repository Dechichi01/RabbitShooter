using UnityEngine;
using System.Collections;
using System;

public class WeaponManager : MonoBehaviour {

	public Transform weaponHold;
	public Weapon startingWeapon;
	Weapon equippedWeapon;

	void Start(){
		if (startingWeapon != null){
			EquipGun(startingWeapon);
		}
	}

	public void EquipGun(Weapon weaponToEquip){
		if (equippedWeapon != null){
			Destroy(equippedWeapon.gameObject);
		}

        equippedWeapon = (Weapon) Instantiate(weaponToEquip);
        equippedWeapon.Equip(weaponHold);
	}

	public void Use(){
		if (equippedWeapon != null){
			equippedWeapon.Use();
		}
	}
}
