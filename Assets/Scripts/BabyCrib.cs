using UnityEngine;
using System.Collections;

public class BabyCrib : Obstacle {

    public float maxNoiseTolerance;
    private float currentNoiseTolerance;

	// Use this for initialization
	void Awake () {
        currentNoiseTolerance = maxNoiseTolerance;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void TakeDamage()
    {

    }
}
