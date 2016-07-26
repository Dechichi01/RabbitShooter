using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
public class Gun : MonoBehaviour {

	public Transform muzzle;
	public Projectile projectile;
	public float msBetweenShots = 100;
	public float muzzleVelocity = 35f;
    public int projectilesPerMag = 6;

    private int projectilesRemaining;
    private bool isReloading;
    public float reloadTime = .3f;

    [Header("Recoil")]
    public float recoil = .2f;

    [Header ("Effects")]
    public Transform shell;
    public Transform shellEjection;
    private MuzzleFlash muzzleFlash;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

	float nextShotTime;

    Vector3 recoilSmoothVel;

    void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        projectilesRemaining = projectilesPerMag;
        PoolManager.instance.CreatePool(projectile.gameObject, 30);
        PoolManager.instance.CreatePool(shell.gameObject, 30);
    }

    void LateUpdate()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothVel, .1f);

        if (!isReloading && projectilesRemaining == 0)
            Reload();
    }

	public void Shoot(){
		if (Time.time > nextShotTime && !isReloading && projectilesRemaining > 0){
            projectilesRemaining--;

			nextShotTime = Time.time + msBetweenShots/1000;
            Projectile newProjectile = PoolManager.instance.ReuseObject(projectile.gameObject, muzzle.position, muzzle.rotation).GetComponent<Projectile>();
			//Projectile newProjectile = (Projectile) Instantiate(projectile, muzzle.position, muzzle.rotation);
			newProjectile.SetSpeed(muzzleVelocity);

            PoolManager.instance.ReuseObject(shell.gameObject, shellEjection.position, shellEjection.rotation);
            //Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();

            transform.localPosition -= Vector3.forward * recoil;

            AudioManager.instance.PlaySound(shootAudio, transform.position);
		}
	}

    public void Reload()
    {
        if (!isReloading && projectilesRemaining != projectilesPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30f;
        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;           
        }
        isReloading = false;
        projectilesRemaining = projectilesPerMag;
    }

}
