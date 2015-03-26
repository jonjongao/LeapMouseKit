/******************************************************************************************
 * Licensed under a Creative Commons Attribution-NonCommercial 4.0 International License.
 * 
 * LeapMouseEditor
 * Last Update : 2015.03.17
 * Developer : Rosa Gao
******************************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(LeapMouseKit))]
public class ALeapMouseEditor : Editor
{
    private LeapMouseKit me;
    private SerializedProperty raycastTarget;
    private SerializedProperty rightHandData;
    private SerializedProperty leftHandData;

    void OnEnable()
    {
        me = (LeapMouseKit)target;
        raycastTarget = serializedObject.FindProperty("raycastTarget");
        rightHandData = serializedObject.FindProperty("rightHandData");
        leftHandData = serializedObject.FindProperty("leftHandData");
    }

    public override void OnInspectorGUI()
    {
        if (me.handState == LeapMouseKit.HandState.Miss)
            GUI.color = Color.red;
        else
            GUI.color = Color.green;
        GUILayout.Box(me.handState.ToString(), EditorStyles.miniButton, GUILayout.ExpandWidth(true));
        GUI.color = Color.white;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Setup"))
        {
            me.menu = LeapMouseKit.Menu.Setup;
        }
        else if (GUILayout.Button("Output"))
        {
            me.menu = LeapMouseKit.Menu.Output;
        }
        EditorGUILayout.EndHorizontal();

        if (me.menu == LeapMouseKit.Menu.Setup)
        {
            //EditorGUILayout.BeginHorizontal();
            me.mouseMode = EditorGUILayout.Toggle("Mouse Mode", me.mouseMode);
            me.debugMode = EditorGUILayout.Toggle("Debug Message", me.debugMode);
            //EditorGUILayout.EndHorizontal();
            me.cursorType = (LeapMouseKit.CursorType)EditorGUILayout.EnumPopup("Cursor Type", me.cursorType);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            if (me.cursorType == LeapMouseKit.CursorType.GameObject)
            {
                me.cursorObject = EditorGUILayout.ObjectField("Cursor Object", me.cursorObject, typeof(GameObject), false) as GameObject;
            }
            else
            {
                EditorGUILayout.BeginVertical();
                me.screenCursorSize = EditorGUILayout.Vector2Field("Cursor Size", me.screenCursorSize);
                me.defaultUiPack = EditorGUILayout.Toggle("Use Default UI Pack", me.defaultUiPack);
                if (!me.defaultUiPack)
                {
                    me.handVisibilityOn = EditorGUILayout.ObjectField("Visibility On", me.handVisibilityOn, typeof(Texture2D), false) as Texture2D;
                    me.handVisibilityOff = EditorGUILayout.ObjectField("Visibility Off", me.handVisibilityOn, typeof(Texture2D), false) as Texture2D;
                    me.handOpen = EditorGUILayout.ObjectField("Hand Open", me.handOpen, typeof(Texture2D), false) as Texture2D;
                    me.handClose = EditorGUILayout.ObjectField("Hand Close", me.handClose, typeof(Texture2D), false) as Texture2D;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(raycastTarget, new GUIContent("Target Layer"));
            me.alphaChannel = EditorGUILayout.Slider("Texture Alpha", me.alphaChannel, 0f, 1f);
            me.enableCursorEvent = EditorGUILayout.Toggle("Enable Event", me.enableCursorEvent);
            me.cursorSticking = EditorGUILayout.Toggle("Stick Enable", me.cursorSticking);


            if (me.cursorSticking)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                me.stickSetup = EditorGUILayout.Foldout(me.stickSetup, "Stick Setting");
                EditorGUILayout.EndHorizontal();
                if (me.stickSetup)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    me.stickOnSpeed = EditorGUILayout.FloatField("Stick Tween Speed", me.stickOnSpeed);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    me.stickBreakDistance = EditorGUILayout.FloatField("Break Distance", me.stickBreakDistance);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    me.stickConfilmTime = EditorGUILayout.FloatField("Event Trigger Time", me.stickConfilmTime);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    me.stickEvent = (LeapMouseKit.HandState)EditorGUILayout.EnumPopup("Event Type", me.stickEvent);
                    EditorGUILayout.EndHorizontal();
                }
            }

            me.rightHandEnable = EditorGUILayout.Toggle("Right Hand Enable", me.rightHandEnable);
            me.leftHandEnable = EditorGUILayout.Toggle("Left Hand Enable", me.leftHandEnable);

        }
        else if (me.menu == LeapMouseKit.Menu.Output)
        {
            EditorGUILayout.Vector3Field("Normalize", me.focusPoint);

            leftHandData.boolValue = EditorGUILayout.Foldout(leftHandData.boolValue, "Left Hand Data");
            if (leftHandData.boolValue)
            {

            }
            rightHandData.boolValue = EditorGUILayout.Foldout(rightHandData.boolValue, "Right Hand Data");
            if (rightHandData.boolValue)
            {
                EditorGUILayout.Vector3Field("World Position", me.worldPosition_right);
                EditorGUILayout.Vector3Field("Screen Position", me.screenPosition_right);
                EditorGUILayout.FloatField("Grab Strength", me.grabStrength_right);
            }
        }

        EditorUtility.SetDirty(me);
        /************************************/
        //EditorGUILayout.LabelField("////////////////////////////////////");
        //DrawDefaultInspector();
    }
}
