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
        Attack = 5,
        ChangeWeapon = 6
    }

    SwipeDirection sSwipeDirection = SwipeDirection.Null;
    public RectTransform aimJoystickRect;
    public RectTransform attackJoystickRect;
    public RectTransform changeWeaponJoystickRect;

    public float minSwipeDistY = 0f;
    public float minSwipeDistX = 0f;

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
                if (RectTransformUtility.RectangleContainsScreenPoint(attackJoystickRect, possibleTouch.position))
                {
                    if (possibleTouch.phase == TouchPhase.Began)
                        sSwipeDirection = SwipeDirection.Attack;
                    break;
                }    
                else if (RectTransformUtility.RectangleContainsScreenPoint(changeWeaponJoystickRect, possibleTouch.position))
                {
                    if (possibleTouch.phase == TouchPhase.Began)
                        sSwipeDirection = SwipeDirection.ChangeWeapon;
                    break;
                }             
                else if (RectTransformUtility.RectangleContainsScreenPoint(aimJoystickRect, possibleTouch.position))
                {
                    touch = possibleTouch;
                    break;
                }
            }
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startPos = touch.position;
                    starTime = Time.time;
                    break;
                case TouchPhase.Ended:
                    float swipeDistVertical = (new Vector3(0, touch.position.y, 0) - new Vector3(0, startPos.y, 0)).magnitude;
                    if (swipeDistVertical > minSwipeDistY)
                    {
                        float swipeValue = Mathf.Sign(touch.position.y - startPos.y);
                        if (swipeValue > 0)
                            sSwipeDirection = SwipeDirection.Jump;

                        else if (swipeValue < 0)//down swipe
                            sSwipeDirection = SwipeDirection.Duck;
                    }
                    float swipeDistHorizontal = (new Vector3(touch.position.x, 0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;
                    if (swipeDistHorizontal > minSwipeDistX)
                    {
                        float swipeValue = Mathf.Sign(touch.position.x - startPos.x);
                        if (swipeValue > 0)//right swipe
                            sSwipeDirection = SwipeDirection.Right;
                        else if (swipeValue < 0)//left swipe
                            sSwipeDirection = SwipeDirection.Left;
                    }
                    break;
            }
        }
    }

    public SwipeDirection GetSwipeDirection() // to be used by Update()
    {
        if (sSwipeDirection != SwipeDirection.Null)//if a swipe is detected
        {
            SwipeDirection etempSwipeDirection = sSwipeDirection;
            sSwipeDirection = SwipeDirection.Null;
            Debug.Log(etempSwipeDirection);
            return etempSwipeDirection;
        }
        else
        {
            return SwipeDirection.Null;//if no swipe was detected
        }
    }
}
