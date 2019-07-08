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

        moveableBlock.XMoves = EditorGUILayout.IntSlider("Total X Axis Moves", moveableBlock.XMoves, 0, 10);
        moveableBlock.YMoves = EditorGUILayout.IntSlider("Total Y Axis Moves", moveableBlock.YMoves, 0, 10);
        moveableBlock.ZMoves = EditorGUILayout.IntSlider("Total Z Axis Moves", moveableBlock.ZMoves, 0, 10);

        EditorGUILayout.Space();

        moveableBlock.XPos = EditorGUILayout.IntSlider("X Starting Position", moveableBlock.XPos, 0, moveableBlock.XMoves);
        moveableBlock.YPos = EditorGUILayout.IntSlider("Y Starting Position", moveableBlock.YPos, 0, moveableBlock.YMoves);
        moveableBlock.ZPos = EditorGUILayout.IntSlider("Z Starting Position", moveableBlock.ZPos, 0, moveableBlock.ZMoves);

        moveableBlock.transform.localPosition = new Vector3(moveableBlock.XPos * moveableBlock.transform.localScale.x,
            moveableBlock.YPos * moveableBlock.transform.localScale.y,
             moveableBlock.ZPos * moveableBlock.transform.localScale.z);

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

            xPoints[0] = moveableBlock.transform.TransformPoint(new Vector3((moveableBlock.transform.localPosition.x - 2 * moveableBlock.XPos) * moveableBlock.transform.localScale.x, (moveableBlock.transform.localPosition.y - moveableBlock.YPos) * moveableBlock.transform.localScale.y + .5f * moveableBlock.transform.localScale.y, (moveableBlock.transform.localPosition.z - moveableBlock.ZPos) * moveableBlock.transform.localScale.z));
            xPoints[1] = moveableBlock.transform.TransformPoint(new Vector3((moveableBlock.XMoves - moveableBlock.XPos) * moveableBlock.transform.localScale.x, (moveableBlock.transform.localPosition.y - moveableBlock.YPos) * moveableBlock.transform.localScale.y + .5f * moveableBlock.transform.localScale.y, (moveableBlock.transform.localPosition.z - moveableBlock.ZPos) * moveableBlock.transform.localScale.z));

            yPoints[0] = moveableBlock.transform.TransformPoint(new Vector3((moveableBlock.transform.localPosition.x - moveableBlock.XPos) * moveableBlock.transform.localScale.x, (moveableBlock.transform.localPosition.y - 2 * moveableBlock.YPos) * moveableBlock.transform.localScale.y + .5f * moveableBlock.transform.localScale.y, (moveableBlock.transform.localPosition.z - moveableBlock.ZPos) * moveableBlock.transform.localScale.z));
            yPoints[1] = moveableBlock.transform.TransformPoint(new Vector3((moveableBlock.transform.localPosition.x - moveableBlock.XPos) * moveableBlock.transform.localScale.x, (moveableBlock.YMoves - moveableBlock.YPos) * moveableBlock.transform.localScale.y + .5f * moveableBlock.transform.localScale.y, (moveableBlock.transform.localPosition.z - moveableBlock.ZPos) * moveableBlock.transform.localScale.z));

            zPoints[0] = moveableBlock.transform.TransformPoint(new Vector3((moveableBlock.transform.localPosition.x - moveableBlock.XPos) * moveableBlock.transform.localScale.x, (moveableBlock.transform.localPosition.y - moveableBlock.YPos) * moveableBlock.transform.localScale.y + .5f * moveableBlock.transform.localScale.y, (moveableBlock.transform.localPosition.z - 2 * moveableBlock.ZPos) * moveableBlock.transform.localScale.z));
            zPoints[1] = moveableBlock.transform.TransformPoint(new Vector3((moveableBlock.transform.localPosition.x - moveableBlock.XPos) * moveableBlock.transform.localScale.x, (moveableBlock.transform.localPosition.y - moveableBlock.YPos) * moveableBlock.transform.localScale.y + .5f * moveableBlock.transform.localScale.y, (moveableBlock.ZMoves - moveableBlock.ZPos) * moveableBlock.transform.localScale.z));

			Handles.color = Color.magenta;
			
            Handles.DrawAAPolyLine(15, xPoints);
            Handles.DrawAAPolyLine(15, yPoints);
            Handles.DrawAAPolyLine(15, zPoints);
        }
    }
}
