﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
public class Gun : MonoBehaviour {

	public Transform muzzle;
	public Projectile projectile;
	public float msBetweenShots = 100;
	public float muzzleVelocity = 35f;

    public Transform shell;
    public Transform shellEjection;

    private MuzzleFlash muzzleFlash;

	float nextShotTime;

    void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
    }

	public void Shoot(){
		if (Time.time > nextShotTime){
			nextShotTime = Time.time + msBetweenShots/1000;
			Projectile newProjectile = (Projectile) Instantiate(projectile, muzzle.position, muzzle.rotation);
			newProjectile.SetSpeed(muzzleVelocity);

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();
		}
	}
}
