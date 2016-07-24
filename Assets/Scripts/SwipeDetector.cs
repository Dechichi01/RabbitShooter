using UnityEngine;
using System.Collections;
 
 public class SwipeDetector : MonoBehaviour
{

    public enum SwipeDirection
    {
        Null = 0, //no swipe detected
        Duck = 1, //swipe down detected
        Jump = 2, //swipe up detected
        Right = 3, //swipe right detected
        Left = 4, //swipe left detected
        Shoot = 5
    }

    SwipeDirection sSwipeDirection = SwipeDirection.Null;
    public RectTransform aimJoystickRect;
    public RectTransform shootJoystickRect;

    public float minSwipeDistY;
    public float minSwipeDistX;

    private Vector2 startPos;
    private float starTime;
    private float touchTimeToShoot = 0.2f;
    private bool isShooting = false;

    void Update()
    {
        //#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = new Touch();
            foreach (Touch possibleTouch in Input.touches)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(shootJoystickRect, possibleTouch.position))
                {
                    if (possibleTouch.phase == TouchPhase.Began)
                        sSwipeDirection = SwipeDirection.Shoot;
                }                    
                else if (RectTransformUtility.RectangleContainsScreenPoint(aimJoystickRect, possibleTouch.position))
                {
                    if (possibleTouch.phase == TouchPhase.Began)
                        sSwipeDirection = SwipeDirection.Right;
                }
            }
        }
    }

    public SwipeDirection GetSwipeDirection() // to be used by Update()
    {
        if (sSwipeDirection != SwipeDirection.Null)//if a swipe is detected
        {
            SwipeDirection etempSwipeDirection = sSwipeDirection;
            sSwipeDirection = SwipeDirection.Null;

            return etempSwipeDirection;
        }
        else
        {
            return SwipeDirection.Null;//if no swipe was detected
        }
    }
}
