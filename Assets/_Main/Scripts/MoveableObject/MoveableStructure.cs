using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableStructure : MonoBehaviour
{
    private List<GameObject> _structures;
    private List<MoveablePlane> _moveablePlanes;

    private void Awake()
    {
        _structures = new List<GameObject>();
        _moveablePlanes = new List<MoveablePlane>();

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

        ReturnLayer();
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
    }

    public void UpdatePlaneColors()
    {
        foreach(MoveablePlane plane in _moveablePlanes) plane.UpdateColor();
    }

}
