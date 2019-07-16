using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MoveableBlock)), CanEditMultipleObjects]
public class MoveableBlock_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MoveableBlock moveableBlock = (MoveableBlock)target;
        
        EditorGUILayout.Space();

        moveableBlock.ShowPreview = EditorGUILayout.Toggle("Show Preview", moveableBlock.ShowPreview);

        EditorGUILayout.Space();

        moveableBlock.XMoves = EditorGUILayout.IntSlider("Total X Axis Moves", moveableBlock.XMoves, 0, 20);
        moveableBlock.YMoves = EditorGUILayout.IntSlider("Total Y Axis Moves", moveableBlock.YMoves, 0, 20);
        moveableBlock.ZMoves = EditorGUILayout.IntSlider("Total Z Axis Moves", moveableBlock.ZMoves, 0, 20);

        EditorGUILayout.Space();

        moveableBlock.XPos = EditorGUILayout.IntSlider("X Starting Position", moveableBlock.XPos, 0, moveableBlock.XMoves);
        moveableBlock.YPos = EditorGUILayout.IntSlider("Y Starting Position", moveableBlock.YPos, 0, moveableBlock.YMoves);
        moveableBlock.ZPos = EditorGUILayout.IntSlider("Z Starting Position", moveableBlock.ZPos, 0, moveableBlock.ZMoves);

        if(!Application.isPlaying)
        {
            moveableBlock.transform.parent.localPosition = new Vector3(moveableBlock.XPos * moveableBlock.MoveDistance, moveableBlock.YPos * moveableBlock.MoveDistance, moveableBlock.ZPos * moveableBlock.MoveDistance);
        }

        if(GUI.changed)
        {
            EditorUtility.SetDirty(moveableBlock);
        }
    }

    public void OnSceneGUI()
    {
        MoveableBlock moveableBlock = (MoveableBlock)target;

        if(moveableBlock.ShowPreview)
        {
            Vector3[] xPoints = new Vector3[2];
            Vector3[] yPoints = new Vector3[2];
            Vector3[] zPoints = new Vector3[2];

            if(!Application.isPlaying)
            {
                xPoints[0] = new Vector3(moveableBlock.transform.parent.position.x - (moveableBlock.XPos * moveableBlock.MoveDistance), moveableBlock.transform.parent.position.y + (moveableBlock.transform.localScale.y / 2), moveableBlock.transform.parent.position.z);
                xPoints[1] = new Vector3(moveableBlock.transform.parent.position.x + ((moveableBlock.XMoves - moveableBlock.XPos) * moveableBlock.MoveDistance), moveableBlock.transform.parent.position.y + (moveableBlock.transform.localScale.y / 2), moveableBlock.transform.parent.position.z);

                yPoints[0] = new Vector3(moveableBlock.transform.parent.position.x, moveableBlock.transform.parent.position.y + (moveableBlock.transform.localScale.y / 2) - (moveableBlock.YPos * moveableBlock.MoveDistance), moveableBlock.transform.parent.position.z);
                yPoints[1] = new Vector3(moveableBlock.transform.parent.position.x, moveableBlock.transform.parent.position.y + (moveableBlock.transform.localScale.y / 2) + ((moveableBlock.YMoves - moveableBlock.YPos) * moveableBlock.MoveDistance), moveableBlock.transform.parent.position.z);

                zPoints[0] = new Vector3(moveableBlock.transform.parent.position.x, moveableBlock.transform.parent.position.y + (moveableBlock.transform.localScale.y / 2), moveableBlock.transform.parent.position.z - (moveableBlock.ZPos * moveableBlock.MoveDistance));
                zPoints[1] = new Vector3(moveableBlock.transform.parent.position.x, moveableBlock.transform.parent.position.y + (moveableBlock.transform.localScale.y / 2), moveableBlock.transform.parent.position.z + ((moveableBlock.ZMoves - moveableBlock.ZPos) * moveableBlock.MoveDistance));
            }

			Handles.color = Color.magenta;
			
            Handles.DrawAAPolyLine(12, xPoints);
            Handles.DrawAAPolyLine(12, yPoints);
            Handles.DrawAAPolyLine(12, zPoints);
        }
    }
}
