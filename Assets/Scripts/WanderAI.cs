using UnityEngine;
using System.Collections;

public class WanderAI : MonoBehaviour {

    public float wanderRadius;
    public float wanderTimer;

    private Transform target;
    private NavMeshAgent agent;
    private float timer;

    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    public Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randomDir = (Random.insideUnitSphere * dist) + origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDir, out navHit, dist, layerMask);

        return navHit.position;
    }
}
