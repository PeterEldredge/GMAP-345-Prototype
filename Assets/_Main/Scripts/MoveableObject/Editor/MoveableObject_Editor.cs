using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MoveableObject)), CanEditMultipleObjects]
public class MoveableObject_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MoveableObject moveableObject = (MoveableObject)target;
        
        EditorGUILayout.Space();

        moveableObject.ShowPreview = EditorGUILayout.Toggle("Show Preview", moveableObject.ShowPreview);

        EditorGUILayout.Space();

        moveableObject.XMoves = EditorGUILayout.IntSlider("Total X Axis Moves", moveableObject.XMoves, 0, 100);
        moveableObject.YMoves = EditorGUILayout.IntSlider("Total Y Axis Moves", moveableObject.YMoves, 0, 100);
        moveableObject.ZMoves = EditorGUILayout.IntSlider("Total Z Axis Moves", moveableObject.ZMoves, 0, 100);

        EditorGUILayout.Space();

        moveableObject.XPos = EditorGUILayout.IntSlider("X Starting Position", moveableObject.XPos, 0, moveableObject.XMoves);
        moveableObject.YPos = EditorGUILayout.IntSlider("Y Starting Position", moveableObject.YPos, 0, moveableObject.YMoves);
        moveableObject.ZPos = EditorGUILayout.IntSlider("Z Starting Position", moveableObject.ZPos, 0, moveableObject.ZMoves);

        if(!Application.isPlaying)
        {
            moveableObject.transform.localPosition = new Vector3(moveableObject.XPos * moveableObject.MoveDistance, moveableObject.YPos * moveableObject.MoveDistance, moveableObject.ZPos * moveableObject.MoveDistance);
        }

        if(GUI.changed)
        {
            EditorUtility.SetDirty(moveableObject);
        }
    }

    public void OnSceneGUI()
    {
        MoveableObject moveableObject = (MoveableObject)target;
        

        if(moveableObject.ShowPreview && !Application.isPlaying)
        {
            Vector3[] xPoints = new Vector3[2];
            Vector3[] yPoints = new Vector3[2];
            Vector3[] zPoints = new Vector3[2];


            xPoints[0] = new Vector3(moveableObject.transform.position.x - (moveableObject.XPos * moveableObject.MoveDistance), moveableObject.transform.position.y, moveableObject.transform.position.z);
            xPoints[1] = new Vector3(moveableObject.transform.position.x + ((moveableObject.XMoves - moveableObject.XPos) * moveableObject.MoveDistance), moveableObject.transform.position.y, moveableObject.transform.position.z);

            yPoints[0] = new Vector3(moveableObject.transform.position.x, moveableObject.transform.position.y - (moveableObject.YPos * moveableObject.MoveDistance), moveableObject.transform.position.z);
            yPoints[1] = new Vector3(moveableObject.transform.position.x, moveableObject.transform.position.y + ((moveableObject.YMoves - moveableObject.YPos) * moveableObject.MoveDistance), moveableObject.transform.position.z);

            zPoints[0] = new Vector3(moveableObject.transform.position.x, moveableObject.transform.position.y, moveableObject.transform.position.z - (moveableObject.ZPos * moveableObject.MoveDistance));
            zPoints[1] = new Vector3(moveableObject.transform.position.x, moveableObject.transform.position.y, moveableObject.transform.position.z + ((moveableObject.ZMoves - moveableObject.ZPos) * moveableObject.MoveDistance));

			Handles.color = Color.magenta;
			
            Handles.DrawAAPolyLine(12, xPoints);
            Handles.DrawAAPolyLine(12, yPoints);
            Handles.DrawAAPolyLine(12, zPoints);
        }
    }
}
