using UnityEngine;
using System.Collections;

public class Exit : MonoBehaviour {

    public bool isConnected = false;
    public bool isDefaultConnection;
    public string[] connectTags;

    public string GetRandomConnectTag()
    {
        return connectTags[Random.Range(0, connectTags.Length)];
    }

    void OnDrawGizmos()
    {
        var scale = 5.0f;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * scale);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - transform.right * scale);
        Gizmos.DrawLine(transform.position, transform.position + transform.right * scale);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * scale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.125f);
    }
}
