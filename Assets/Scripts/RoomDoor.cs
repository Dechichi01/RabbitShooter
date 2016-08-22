using UnityEngine;
using System.Collections;

public class RoomDoor : MonoBehaviour {

    public Transform hinge;
    public bool isOpen;

    public int positionInMap;

    public void OpenDoor()
    {
        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(RotateDoor(Quaternion.Euler(0f, 90f,0f)));
        }
    }

    public void CloseDoor()
    {
        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(RotateDoor(Quaternion.Euler(0f, 180f,0f)));
        }
    }

    IEnumerator RotateDoor(Quaternion endRotation)
    {
        float percent = 0;
        float openDoorTime = 1.5f;
        float openDoorSpeed = 1 / openDoorTime;

        Quaternion startRotation = hinge.localRotation;
        while (percent<=1)
        {
            percent += openDoorSpeed * Time.deltaTime;
            Quaternion rot = Quaternion.Lerp(startRotation, endRotation, percent);
            hinge.localRotation = rot;
            yield return null;
        }
        hinge.localRotation = endRotation;
        isOpen = !isOpen;
    }
}

