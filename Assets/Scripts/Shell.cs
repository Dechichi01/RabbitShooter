using UnityEngine;
using System.Collections;

public class Shell : PoolObject {

    public Rigidbody myRigidbody;
    public float forceMin;
    public float forceMax;

    float lifetime = 1.5f;
    float fadetime = 2;

    private Color initialColor;
	// Use this for initialization

    void Awake()
    {
        Material mat = GetComponent<Renderer>().material;
        initialColor = mat.color;
    }

	void Start () {
        AddForces();
        StartCoroutine(Fade());
	}

    public override void OnObjectReuse()
    {
        GetComponent<Renderer>().material.color = initialColor;
        AddForces();
        StartCoroutine(Fade());
    }
    private void AddForces()
    {
        float force = Random.Range(forceMin, forceMax);
        myRigidbody.AddForce(transform.right * force);
        myRigidbody.AddTorque(Random.insideUnitSphere * force);
    }
    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifetime);

        float percent = 0;
        float fadeSpeed = 1 / fadetime;
        Material mat = GetComponent<Renderer>().material;

        while (percent<1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }
        Debug.Log("ok");
        Destroy();
    }
}
