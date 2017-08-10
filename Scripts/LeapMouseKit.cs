/*****************************************************************************************
 * Licensed under a Creative Commons Attribution-NonCommercial 4.0 International License.
 * 
 * LeapMouseKit
 * Last Update : 2015.03.17
 * Developer : Rosa Gao
*****************************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class LeapMouseKit : MonoBehaviour
{
    #region Editor
    public Menu menu;
    public enum Menu { Setup, Output }
    public bool stickSetup;
    public bool rightHandData;
    public bool leftHandData;
    public bool defaultUiPack;
    #endregion

    #region Output
    public bool trigger;
    public Vector3 focusPoint;
    public GameObject cursorObject;
    public bool enableCursorEvent;
    public float alphaChannel;
    public bool sticking;

    //Right hand data
    public static Vector3 runtimePosition_right;
    public static Vector3 worldPosition_right;
    public static Vector2 screenPosition_right;
    public static float grabStrength_right;
    //Left hand data
    public static Vector3 runtimePosition_left;
    public static Vector3 worldPosition_left;
    public static Vector2 screenPosition_left;
    public static float grabStrength_left;
    #endregion

    #region Setup
    private Leap.Controller controller;
    public CursorType cursorType;
    public enum CursorType { Gui, GameObject };
    public bool mouseMode;
    public bool debugMode;
    public bool cursorSticking;
    public LayerMask raycastTarget;
    public bool rightHandEnable;
    public bool leftHandEnable;
    public float stickOnSpeed = 500f;
    public float stickBreakDistance = 3f;
    public float stickConfilmTime = 3f;
    public HandState stickEvent;
    public float stickTime;
    #endregion

    #region Texture
    public Vector2 screenCursorSize;
    public Texture2D handVisibilityOn;
    public Texture2D handVisibilityOff;
    public Texture2D handOpen;
    public Texture2D handClose;
    public Texture2D closeMouse;
    public Texture2D openMouse;
    #endregion

    public HandState handState;
    public enum HandState { Hold, Circle, ScreenTap, KeyTap, Swipe, HandOpen, HandClose, HandForward, HandBack, HandUp, HandDown, HandLeft, HandRight, Show, Miss }
    private Texture2D textureStorage_right;
    private GameObject cursorObject_right;
    public GameObject lastStickTarget;
    public static HandSide handSide;
    public enum HandSide { Left, Right };

    void Awake()
    {
        if (defaultUiPack)
        {
            handVisibilityOn = Resources.Load("ic_visibility_on", typeof(Texture2D)) as Texture2D;
            handVisibilityOff = Resources.Load("ic_visibility_off", typeof(Texture2D)) as Texture2D;
            handOpen = Resources.Load("handOpen_right", typeof(Texture2D)) as Texture2D;
            handClose = Resources.Load("handClose_right", typeof(Texture2D)) as Texture2D;
            closeMouse = Resources.Load("mouseCursor_close", typeof(Texture2D)) as Texture2D;
            openMouse = Resources.Load("mouseCursor_open", typeof(Texture2D)) as Texture2D;
        }

        if (cursorType == CursorType.GameObject)
        {
            //Get cursor prefab
            if (cursorObject != null)
            {
                GameObject _r = Instantiate(cursorObject, new Vector3(0f, 0f, transform.position.z), Quaternion.identity) as GameObject;
                cursorObject_right = _r;
                cursorObject_right.name = "Cursor Object Right";
            }
            else
                Debug.LogError("Can't find cursor prefab");
        }

        textureStorage_right = handVisibilityOff;
    }

    void Start()
    {
        /*****************************************************************
         * Built-in gesture setup
         * **************************************************************/
        controller = new Controller();
        controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
        controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
        controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
        controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
        //Default is 150f
        controller.Config.SetFloat("Gesture.Swipe.MinLength", 100f);
        //Default is 1000f
        controller.Config.SetFloat("Gesture.Swipe.MinVelocity", 1250f);
        controller.Config.Save();
        if (debugMode)
            Debug.LogWarning("Leap Motion config saved");
    }

    void FixedUpdate()
    {
        if (!mouseMode)
        {
            Frame frame = controller.Frame();
            InteractionBox box = frame.InteractionBox;
            Vector _rightHand = new Vector();
            Vector _leftHand = new Vector();

            #region Find Hand
            if (frame.Hands.Count != 0)
            {
                ActionTrigger(HandState.Show);

                #region Basic Gestures
                foreach (Gesture g in frame.Gestures())
                {
                    if (g.IsValid)
                    {
                        if (g.Type == Gesture.GestureType.TYPE_CIRCLE)
                        {
                            ActionTrigger(HandState.Circle);
                        }
                        if (g.Type == Gesture.GestureType.TYPE_SCREEN_TAP)
                        {
                            ActionTrigger(HandState.ScreenTap);
                        }
                        if (g.Type == Gesture.GestureType.TYPE_KEY_TAP)
                        {
                            ActionTrigger(HandState.KeyTap);
                        }
                        if (g.Type == Gesture.GestureType.TYPE_SWIPE)
                        {
                            ActionTrigger(HandState.Swipe);
                        }
                    }
                }
                #endregion

                foreach (Hand h in frame.Hands)
                {
                    #region Right Hand
                    if (h.IsRight && rightHandEnable)
                    {
                        if (debugMode)
                        {
                            Debug.Log("Right Hand Grab Strength: " + h.GrabStrength);
                        }
                        grabStrength_right = h.GrabStrength;
                        _rightHand = box.NormalizePoint(h.PalmPosition);

                        //Accuracy more than 0.1
                        if (h.Confidence > .1f)
                        {
                            if (h.PalmVelocity.z < -200f && h.GrabStrength < .6)
                            {
                                ActionTrigger(HandState.HandForward);
                            }
                            else if (h.PalmVelocity.z > 100f || Mathf.RoundToInt(h.GrabStrength) == 1)
                            {
                                ActionTrigger(HandState.HandBack);
                            }
                            else if (h.PalmVelocity.y > 400f)
                            {
                                ActionTrigger(HandState.HandUp);
                            }
                            else if (h.PalmVelocity.y < -600f && h.GrabStrength > .6)
                            {
                                ActionTrigger(HandState.HandDown);
                            }

                            if (h.GrabStrength > .6)
                            {
                                ActionTrigger(HandState.HandClose);
                            }
                            else if (h.GrabStrength < .6)
                            {
                                ActionTrigger(HandState.HandOpen);
                            }
                        }
                    }
                    #endregion
                    #region Left Hand

                    #endregion
                }

                #region Update Output Data
                //Runtime
                runtimePosition_right = Camera.main.ViewportToWorldPoint(new Vector3(_rightHand.x, _rightHand.y, Camera.main.nearClipPlane));
                if (!sticking)
                {
                    //World position
                    worldPosition_right = transform.position + new Vector3(runtimePosition_right.x, runtimePosition_right.y, 0);
                    //Screen position
                    screenPosition_right = new Vector2(UnityEngine.Screen.width * _rightHand.x, UnityEngine.Screen.height * _rightHand.y);
                    //Focus point
                    focusPoint = new Vector3(_rightHand.x, _rightHand.y, 0);
                }
                #endregion
            }
            #endregion

            #region Lost Hand
            else
            {
                ActionTrigger(HandState.Miss);

                //World position
                worldPosition_right = new Vector3(0, 0, transform.position.z);
                //Screen position
                screenPosition_right.x = UnityEngine.Screen.width * 0.5f;
                screenPosition_right.y = UnityEngine.Screen.height * 0.5f;
                //Focus point
                focusPoint = Vector3.zero;
            }
            #endregion
        }
        else
        {
            #region MouseMode
            runtimePosition_right = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            if (!sticking)
            {
                //World position
                worldPosition_right = new Vector3(runtimePosition_right.x, runtimePosition_right.y, transform.position.z);
                //Screen position
                screenPosition_right = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                //Focus point
                Vector3 _c = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(transform.position.z)));
                focusPoint = new Vector3(Mathf.Clamp(_c.x, 0, 1), Mathf.Clamp(_c.y, 0, 1), Mathf.Clamp(_c.z, 0, 1));
            }
            //if (Input.GetMouseButtonDown(0))
            //{
            //    //Debug.Log("Shoot");
            //    ActionTrigger(HandState.HandClose);
            //    trigger = true;
            //}
            if (Input.GetMouseButton(0))
            {
                //Debug.Log("Hold");
                ActionTrigger(HandState.HandOpen);
                trigger = false;
            }
            if (Input.GetMouseButtonUp(0))
            {
                //Debug.Log("Release");
                ActionTrigger(HandState.HandClose);
                trigger = true;
            }

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                //Debug.Log("Wheel Up");
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                //Debug.Log("Wheel Down");
                ActionTrigger(HandState.HandDown);
            }
            #endregion
        }

        //Update position here
        if (cursorType == CursorType.GameObject)
        {
            if (rightHandEnable)
            {
                cursorObject_right.transform.position = worldPosition_right;
            }
        }
    }

    void OnGUI()
    {
        if (cursorType == CursorType.Gui)
        {
            GUI.color = new Color(1, 1, 1, alphaChannel);
            if (rightHandEnable)
                GUI.DrawTexture(new Rect(Mathf.Clamp(screenPosition_right.x, 0, UnityEngine.Screen.width + (screenCursorSize.x * 0.5f)) - screenCursorSize.x * 0.5f,
                    Mathf.Clamp(UnityEngine.Screen.height - screenPosition_right.y, 0, UnityEngine.Screen.height + (screenCursorSize.y * 0.5f)) - screenCursorSize.y * 0.5f,
                    screenCursorSize.x, screenCursorSize.y),
                    textureStorage_right);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(worldPosition_right, transform.up * 10f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(worldPosition_right, transform.right * 10f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(worldPosition_right, transform.forward * 10f);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(worldPosition_right, 0.5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(worldPosition_right, 0.5f);
        Gizmos.DrawRay(worldPosition_right, transform.forward * 100f);
    }

    bool StickToTarget(RaycastHit hit)
    {
        if (hit.collider.GetComponent<Collider>())
        {
            if (hit.collider.gameObject != lastStickTarget)
            {
                lastStickTarget = hit.collider.gameObject;
                stickTime = 0f;
            }
            //Debug.Log(hit.collider.collider);
            Vector3 targetCenter = hit.collider.transform.position;
            Vector3 targetScreenCenter = Camera.main.WorldToScreenPoint(targetCenter);
            // _rightMouseX = targetScreenCenter.x;
            // _rightMouseY = targetScreenCenter.y;
            Vector3 move = Vector2.MoveTowards(new Vector2(screenPosition_right.x, screenPosition_right.y), new Vector3(targetScreenCenter.x, targetScreenCenter.y), Time.deltaTime * stickOnSpeed);
            Vector3 screenMove = Vector2.MoveTowards(new Vector2(worldPosition_right.x, worldPosition_right.y), new Vector3(targetCenter.x, targetCenter.y), Time.deltaTime * stickOnSpeed);
            screenPosition_right.x = move.x;
            screenPosition_right.y = move.y;
            worldPosition_right.x = screenMove.x;
            worldPosition_right.y = screenMove.y;
            float distance = Vector2.Distance(new Vector2(targetCenter.x, targetCenter.y), new Vector2(runtimePosition_right.x, runtimePosition_right.y));
            //Debug.Log(distance);
            if (distance > stickBreakDistance)
            {
                stickTime = 0f;
                return false;
            }

            if (stickTime > stickConfilmTime)
            {
                hit.collider.SendMessage("OnLeap" + stickEvent, hit.point, SendMessageOptions.DontRequireReceiver);
                if (debugMode)
                {
                    Debug.Log("Triggered Action" + stickEvent.ToString());
                }
                stickTime = 0f;
            }
            else
            {
                stickTime += Time.deltaTime;
            }
            return true;
        }
        else
            return false;
    }

    void ActionTrigger(HandState g)
    {
        #region Send Message
        handState = g;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(worldPosition_right, transform.forward, 100f, raycastTarget);

        if (g == HandState.Miss)
            textureStorage_right = handVisibilityOff;
        else if (g == HandState.HandOpen)
            textureStorage_right = handOpen;
        else if (g == HandState.HandClose)
            textureStorage_right = handClose;

        foreach (RaycastHit r in hits)
        {
            if (cursorSticking)
            {
                sticking = StickToTarget(r);
            }

            foreach (string a in Enum.GetNames(typeof(HandState)))
            {
                if (a == g.ToString())
                {
                    trigger = true;
                    r.collider.SendMessage("OnLeap" + a, r.point, SendMessageOptions.DontRequireReceiver);

                    if (debugMode)
                    {
                        Debug.Log("Triggered Action" + a);
                    }
                }
            }

        }
        #endregion
    }

    public static Vector3 GetWorldPosition(LeapMouseKit.HandSide hand)
    {
        if (hand == LeapMouseKit.HandSide.Left)
            return worldPosition_left;
        else
            return worldPosition_right;
    }

    public static Vector3 GetScreenPosition(LeapMouseKit.HandSide hand)
    {
        if (hand == LeapMouseKit.HandSide.Left)
            return screenPosition_left;
        else
            return screenPosition_right;
    }

    public static float GetGrabStrength(LeapMouseKit.HandSide hand)
    {
        if (hand == LeapMouseKit.HandSide.Left)
            return grabStrength_left;
        else
            return grabStrength_right;
    }
}
