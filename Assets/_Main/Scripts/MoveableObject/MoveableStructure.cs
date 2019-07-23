using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableStructure : MonoBehaviour
{
    private List<GameObject> _structures;
    private List<MoveablePlane> _moveablePlanes;
    private List<MoveableStructure> _moveableStructures;

    private void Awake()
    {
        _structures = new List<GameObject>();
        _moveablePlanes = new List<MoveablePlane>();
        _moveableStructures = new List<MoveableStructure>();

        SearchForMoveableStructures(transform);

        for(int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            MoveablePlane moveablePlane = child.GetComponent<MoveablePlane>();

            if(moveablePlane)
            {
                _moveablePlanes.Add(moveablePlane);
            }
            else
            {
                _structures.Add(child);
            }
        }
    }

    private void Start()
    {
        ReturnLayer();
    }

    private void SearchForMoveableStructures(Transform parentTransform) //This is so cancer
    {
        foreach(Transform childTransform in parentTransform)
        {
            MoveableStructure moveableStructure = childTransform.gameObject.GetComponent<MoveableStructure>();
            if(moveableStructure)
            {
                _moveableStructures.Add(moveableStructure);
            }
            else
            {
                SearchForMoveableStructures(childTransform);
            }
        }
    }

    public void HideChildrenFromRaycast()
    {
        foreach(GameObject child in _structures)
        {
            child.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        foreach(MoveablePlane plane in _moveablePlanes)
        {
            plane.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        foreach(MoveableStructure moveableStructure in _moveableStructures)
        {
            moveableStructure.HideChildrenFromRaycast();
        }
    }

    public void ReturnLayer()
    {
        foreach(GameObject child in _structures)
        {
            child.layer = LayerMask.NameToLayer("Default");
        }

        foreach(MoveablePlane plane in _moveablePlanes)
        {
            plane.gameObject.layer = LayerMask.NameToLayer("MoveablePlane");
        }

        foreach(MoveableStructure moveableStructure in _moveableStructures)
        {
            moveableStructure.ReturnLayer();
        }
    }

    public void UpdatePlaneColors()
    {
        foreach(MoveablePlane plane in _moveablePlanes) plane.UpdateColor();
    }

}
