using UnityEngine;
using System.Collections;

public class PoolObject : MonoBehaviour {

	public virtual void OnObjectReuse()
    {

    }

    protected virtual void Destroy(float delay = 0)
    {
        Invoke("Inactivate", delay);
    }

    private void Inactivate()
    {
        gameObject.SetActive(false);
    }
}
